import random
from hashlib import sha256


class ECDSA:
    def __init__(self):
        self.a = 0
        self.b = 7
        self.p = 2 ** 256 - 2 ** 32 - 2 ** 9 - 2 ** 8 - 2 ** 7 - 2 ** 6 - 2 ** 4 - 1
        self.n = 115792089237316195423570985008687907852837564279074904382605163141518161494337
        self.G = {
            "x":55066263022277343669578718895168534326250603453777594175500187360389116729240,
            "y":32670510020758816978083085130507043184471273380659243275938904335757337482424
        }
    
    def generalasi_pont(self):
        return self.G
    def mod(self,a):
        return a % self.p
    def duplaz(self, pont):
        Lejt_x = (3*pont["x"]**2 + self.a) 
        Lejt_y = pow((2*pont["y"]),-1,self.p)
        Lejtes = Lejt_x * Lejt_y
        x = (Lejtes*Lejtes - (2*pont["x"]))
        y = (Lejtes*(pont["x"] - x) - pont["y"])
        x = self.mod(x)
        y = self.mod(y)
        return {"x": x, "y": y}

    def osszead(self, pont1, pont2):
        if pont1 == pont2:
            return self.duplaz(pont1)
        Lejt_y = (pont1["y"] - pont2["y"]) 
        Lejt_x = pow(pont1["x"]-pont2["x"],-1,self.p)
        Lejtes = Lejt_y * Lejt_x
        x = (Lejtes*Lejtes - pont1["x"] - pont2["x"])
        y = (Lejtes * (pont1["x"]-x) - pont1["y"])
        x = self.mod(x)
        y = self.mod(y)
        return {"x": x, "y": y}

    def szoroz(self, n, pont):
        jelenlegi_pont = pont
        for b in '{0:b}'.format(n)[1:]:
            jelenlegi_pont = self.duplaz(jelenlegi_pont)
            if int(b):
                jelenlegi_pont = self.osszead(jelenlegi_pont, pont)
        return jelenlegi_pont

    def kulcsGeneralas(self):
        SK = random.randint(1, self.n - 1)
        PK = self.szoroz(SK, self.G)
        return SK, PK

    def alair(self, SK, hash):
        k = int(hex(random.randint(1, self.n-1)),16)
        r = (self.szoroz(k, self.G)["x"]) % self.n
        s = ((hash + SK * r) * pow(k,-1,self.n)) % self.n
        return {"r": r, "s": s}

    def ellenoriz(self, PK, signature, hash):
        u1 = self.szoroz(pow(signature["s"], -1, self.n)*hash, self.G)
        u2 = self.szoroz((pow(signature["s"], -1, self.n) * signature["r"]), PK)
        u3 = self.osszead(u1, u2)
        print(u3)
        print(signature)
        return u3["x"] == signature["r"]


def main():
    txt = "\nEliptikus görbe\n"
    print(txt)
    e_gorbe = ECDSA()
    SK, PK = e_gorbe.kulcsGeneralas()
    G=e_gorbe.generalasi_pont()
    print("Privát kulcs: "+str(SK))
    print("Publikus kulcs (egy pont a görbén): "+str(PK))
    print("Generálási pont (egy pont a görbén): "+str(G))
    hash = sha256(txt.encode('UTF-8')).hexdigest()
    hash = int(hash,16)
    print("Hashelt üzenet: "+str(hash)+"\n")
    alairas = e_gorbe.alair(SK, hash)
    if(e_gorbe.ellenoriz(PK, alairas, hash)):
        print("Valid Aláirás")
    else:
        print("Nem Valid Aláirás")

if __name__ == "__main__":
    main()
