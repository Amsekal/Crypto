using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
namespace blockchain
{


    class Block
    {
        public int id;
        public string transaction = "";
        public string hash;
        public string prev_hash="";
        public int nonce = 0;

        public Block(int id_n, string trans, string hs)
        {
            id = id_n;
            transaction = trans;
            hash = hs;
        }

        public void mineBlock()
        {
            var data = this.transaction+this.nonce.ToString();
            while (this.nonce < 10000)
            {
                var a = Hashing.ToSHA256(data);
                if(a[0].ToString()=="0" && a[1].ToString()=="0" && a[2].ToString() == "0")
                {
                    this.hash = a;
                    break;
                }
                else
                {
                    this.nonce++;
                    data = this.transaction + this.nonce.ToString();
                }
            }
        }
    }

    class User
    {
        public int id;
        public RSA rsa;
        public float money;

        public User(int id_n, RSA keys, float money_n)
        {
            id = id_n;
            rsa = keys;
            money = money_n;
        }

    }

    class MTree
    {
        public string hash;
        public MTree Left;
        public MTree Right;

        public MTree(string h)
        {
            hash = h;
        }
        public MTree(MTree a, MTree b,string h)
        {
            Left = a;
            Right = b;
            hash = h;
        }

        public string makeNode()
        {
            string newHash="";
            newHash = Left.hash + Right.hash;
            return Hashing.ToSHA256(newHash);
        }
    }


    class Hashing
    {
        public static string ToSHA256(string s)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(s));

            var sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
    class Program
    {

        static void Main(string[] args)
        {
            List<MTree> mTrees=new List<MTree>();
            var transactionnumber = 0;
            var currentblock = 1;
            List<Block> BlockChain = new List<Block>();
            User[] users = { new User(0, new RSACryptoServiceProvider(2048), 10.5f), new User(1, new RSACryptoServiceProvider(2048), 5f) };


            BlockChain.AddRange(new List<Block>
            {
                new Block(0,"Origo",Hashing.ToSHA256("Origo")),
                new Block(1,"","")

            });

            bool ok = true;

            var csp = new RSACryptoServiceProvider(2048);

            while (ok == true)
             {

                if(transactionnumber==2)
                {
                    
                    transactionnumber = 0;
                    currentblock++;
                    BlockChain.AddRange(new List<Block>
                    { 
                        new Block(currentblock,"","")

                    });
                    BlockChain[currentblock-1].prev_hash = BlockChain[currentblock-2].hash;
                }


                 Console.Write("Press\n1: Show Blocks\n2: Show users\n3: Make a transaction\n");
                 var input = Console.ReadLine();

                if (input == "0")
                {
                    ok = false;
                }
                 if(input=="1")
                     foreach (Block block in BlockChain)
                     {
                         Console.WriteLine(block.id + " " + block.transaction + " " + block.hash+" "+block.nonce /*+ " "+ block.prev_hash*/);
                     }
                 if(input=="2")
                     foreach (User user in users)
                     {
                         Console.WriteLine(user.id + " " + user.money );
                     }
                 Console.WriteLine("\n");

                if(input=="3")
                {
                    Console.WriteLine("Who initiates transaction:\n");
                    var senderId = Int32.Parse(Console.ReadLine());

                    Console.WriteLine("Who is the reciever:\n");
                    var recieverId = Int32.Parse(Console.ReadLine());

                    Console.WriteLine("The sum:\n");
                    var sum = Int32.Parse(Console.ReadLine());

                    if (senderId >= 0 && senderId < users.Length && recieverId >= 0 && recieverId < users.Length && users[senderId].money >= sum)
                    {
                        users[senderId].money -= sum;
                        users[recieverId].money += sum;

                        string data = users[senderId].id + " sends " + sum + " to " + users[recieverId].id;

                        Console.WriteLine(data);


                        var privKey = users[senderId].rsa.ExportParameters(true);
                        var pubKey = users[senderId].rsa.ExportParameters(false);


                        csp = new RSACryptoServiceProvider();
                        csp.ImportParameters(pubKey);

                        var plainTextData = Hashing.ToSHA256(data);

                        var bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(plainTextData);

                        var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);

                        var cypherText = Convert.ToBase64String(bytesCypherText);

                        csp = new RSACryptoServiceProvider();
                        csp.ImportParameters(privKey);

                        bytesPlainTextData = csp.Decrypt(bytesCypherText, false);

                        plainTextData = System.Text.Encoding.Unicode.GetString(bytesPlainTextData);


                        //Console.WriteLine(cypherText + "\n\n" + Hashing.ToSHA256(data)+"\n\n"+plainTextData);
                        if(Hashing.ToSHA256(data)==plainTextData)
                        {
                            mTrees.AddRange(new List<MTree>
                            {
                                new MTree(Hashing.ToSHA256(data))
                            });
                            Console.WriteLine("RSA Succes");
                            BlockChain[currentblock].transaction += data;
                            transactionnumber++;
                        }



                        if(transactionnumber==2)
                        {
                            BlockChain[currentblock].mineBlock();
                        }

                    }
                    else
                    {
                        Console.WriteLine("Error\n");
                    }


                }
                if(input=="4")
                {
                    /*foreach (MTree tree in mTrees)
                    {
                        Console.WriteLine(tree.hash);
                    }*/
                    if(mTrees.Count==2)
                    {
                        mTrees.AddRange(new List<MTree>
                            {
                                new MTree(mTrees[0],mTrees[1],Hashing.ToSHA256(mTrees[0].hash+mTrees[1].hash))
                            }) ;
                        Console.WriteLine(mTrees[2].hash);
                    }

                    if (mTrees.Count == 4)
                    {
                        mTrees.AddRange(new List<MTree>
                            {
                                new MTree(mTrees[0],mTrees[1],Hashing.ToSHA256(mTrees[0].hash+mTrees[1].hash)),
                                new MTree(mTrees[2],mTrees[3],Hashing.ToSHA256(mTrees[2].hash+mTrees[3].hash))
                            });


                        mTrees.AddRange(new List<MTree>
                        {  new MTree(mTrees[4], mTrees[5], Hashing.ToSHA256(mTrees[4].hash + mTrees[5].hash))
                    });

                        Console.WriteLine(mTrees[6].hash);
                    }
                }
            }
            
        }
    }
}
