﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Krypto.Operations.Crypto_Operations
{
    public enum GeneratorSearch : ushort
    {
        Brute = 1,
        PollardRho = 2,
        IndexCalculator = 3,
        IndexCandidates = 4
    }

    public enum EulerPhiSelection : ushort
    {
        Brute = 1,
        Mobius = 2,
        Classic = 3
    }

    public enum FactorizationSelection : ushort
    {
        Brute = 1,
        PollarRho = 2,
        BabyStepGiantStep = 3,
        LenstraEllipticCurveFactorization = 4
    }

    public class CryptoOperations
    {


        public CryptoOperations()
        {
            Initialization();
        }

        private void Initialization()
        {
        }

        public static BigInteger BabyStepGiantStep(BigInteger modulo, BigInteger generator, BigInteger b)
        {
            BigInteger order = modulo - 1;
            BigInteger h = Intermediaries.SqrtFast(order);

            List<BigInteger> BabyStep = new List<BigInteger>();

            for (BigInteger j = 0; j < h; j++)
            {
                BabyStep.Add(Intermediaries.PowMod(generator, j, modulo));
            }

            BigInteger intermediary = Intermediaries.PowMod(generator, order - h, modulo);
            List<BigInteger> GiantStep = new List<BigInteger>();

            for (BigInteger i = 0; i < h; i++)
            {
                GiantStep.Add((b*Intermediaries.PowMod(intermediary,i,modulo))% modulo);
            }

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    if (BabyStep[j] == GiantStep[i])
                    {
                        return (i * h + j);
                    }
                }
            }
            return 0;
        }

        private static BigInteger FindGenerator(BigInteger modulus)
        {
            var rand = new Random();
            bool flag = true;

            BigInteger temp;
            BigInteger potentialGenerator = 1;

            for (BigInteger i = 0; (i < modulus - 1) && flag; i++)
            {
                potentialGenerator = rand.NextInt64(2, (long)modulus);

                for (BigInteger j = 1; j < modulus ; j++)
                {
                    temp = Intermediaries.PowMod(potentialGenerator, j, modulus);
                    if (temp ==1 && (j != (modulus - 1)))
                    {
                        break;
                    }

                    if (temp == 1 && j == (modulus - 1))
                    {
                        flag = false;
                        break;
                    }
                
                }
            }

            /* Check
            for (BigInteger i = 1; (i < modulus); i++)
            {
                Console.WriteLine(Intermediaries.PowMod(potentialGenerator, i, modulus));
            }
            */

            return potentialGenerator;
        }

        public static  BigInteger powMod(BigInteger x, BigInteger y, BigInteger modulo)
        {
            return (Intermediaries.PowMod(x, y, modulo));
        }

        public static  BigInteger quadraticSieve(BigInteger n)
        {
            BigInteger limit = Intermediaries.SqrtFast(n);
            BigInteger[] primes = generatePrimes(limit);

            BigInteger[] factors = new BigInteger[primes.Length];
            BigInteger[] exponents = new BigInteger[primes.Length];
            int k = primes.Length;



            while (true)
            {
                BigInteger a = Intermediaries.getRandomBigInteger(2, n - 1);
                BigInteger x = Intermediaries.PowMod(a, 2, n);
                BigInteger y = x;

                for (int i = 0; i < k; i++)
                {
                    BigInteger p = primes[i];
                    int e = 0;
                    while (BigInteger.Remainder(y, p) == 0)
                    {
                        y = BigInteger.Divide(y, p);
                        e++;
                    }

                    factors[i] = Intermediaries.PowMod(primes[i], ((exponents[i] + e) % 2), n);
                    exponents[i] = (exponents[i] + e) % 2;
                }

                if (y == 1)
                {
                    BigInteger factor = 1;

                    for (int i = 0; i < k; i++)
                    {
                        factor = BigInteger.Multiply(factor, Intermediaries.PowMod(factors[i], exponents[i], n));
                    }

                    return factor;
                }
            }
        }

        private static BigInteger[] generatePrimes(BigInteger limit)
        {
            bool[] sieve = new bool[(int)BigInteger.Remainder(limit + 1, int.MaxValue)];

            sieve[0] = sieve[1] = true;
            for (int i = 2; i * i <= limit; i++)
            {
                if (!sieve[i])
                {
                    for (int j = i * i; j < (int)BigInteger.Remainder(limit + 1, int.MaxValue); j += i)
                    {
                        sieve[j] = true;
                    }
                }
            }


            BigInteger[] primes = new BigInteger[(int)limit + 1];
            int k = 0;

            for (int i = 2; i <= limit; i++)
            {
                if (!sieve[i])
                {
                    primes[k++]= i;
                }
            }
            Array.Resize(ref primes, k);

            return primes;
        }

        /// <summary>
        /// Pollard Rho algorithm, returns a field of all factors, not all are primes, some may be composites -> need to fix
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static BigInteger[] pollardRho(BigInteger input)
        {
            List<BigInteger> factors = new List<BigInteger>();
            BigInteger[] temp = new BigInteger[2];
            temp[0] = input;
            temp[1] = input;

            temp = pollard(temp[0], temp[1]);
            factors.Add(temp[0]);
            while (temp[1] != (BigInteger)1)
            {
                temp = pollard(temp[1], temp[1]);

                factors.Add(temp[0]);
            }  


            BigInteger[] res = new BigInteger[factors.Count];
            for (int i = 0; i < factors.Count; i++)
            {
                res[i] = factors[i];
            }

            return res;
        }

        private static BigInteger[] pollard(BigInteger input, BigInteger modulo)
        {
            BigInteger x, y;
            BigInteger c, d = 1;

            if (input % 2 == 0)
            {
                return new BigInteger[] { 2, modulo / 2 };
            }

            x = Intermediaries.getRandomBigInteger(2, input - 1);
            y = x;

            c = Intermediaries.getRandomBigInteger(2, input - 1);

            BigInteger index = 0;
            while (d == 1)
            {
                x = CryptoOperations.fX(x, c, modulo);
                y = CryptoOperations.gx(y, c, modulo);

                d = BigInteger.GreatestCommonDivisor(abs(x - 1), modulo);

                index++;
                if (index > 10000)
                {
                    c++;
                    if (d != 1)
                    {
                        if (c > input || MillerRabin(d, 100))
                        {
                            return new BigInteger[] { d, 1 };
                        }
                    }

                    index = 0;
                }
            }




            return new BigInteger[] { d, modulo / d };
        }

        /// <summary>
        /// Miller rabin primality test
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns></returns>
        private static bool MillerRabin(BigInteger candidate, int numberOfIterations)
        {
            if (candidate % 2 == 0)
            {
                return false;
            }

            for (int j = 0; j < numberOfIterations; j++)
            {
                BigInteger a = Intermediaries.getRandomBigInteger(2, candidate - 2);
                BigInteger[] ds = getMillerRabinIntermediaries(candidate);//d,s

                var variable = Intermediaries.PowMod(a, ds[0], candidate);
                if (variable != 1 && variable != candidate - 1)
                {
                    return false;
                }


                for (int i = 0; i < ds[1]; i++)
                {
                    variable = Intermediaries.PowMod(variable, 2, candidate);
                    if (variable != 1 && variable != candidate - 1)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        /// <summary>
        /// returns d and s, where n - 1 = 2^s * d mod n
        /// </summary>
        /// <returns></returns>
        private static BigInteger[] getMillerRabinIntermediaries(BigInteger n)
        {
            BigInteger s, d;

            s = Intermediaries.getRandomBigInteger(2, n - 2);

            d = (n - 1) / Intermediaries.PowMod(2, s, n);

            return new BigInteger[]{d, s};
        }

        /// <summary>
        /// pollard[0] == factor
        /// pollard[1] == remainder to be factored, if 1 than it is finished
        /// </summary>
        /// <param name="input"></param>
        /// <param name="modulo"></param>
        /// <returns></returns>
        private static BigInteger abs(BigInteger input)
        {
            if (input < 0)
            {
                return -input;
            }

            return input;
        }

        /// <summary>
        /// x^2 + c mod n
        /// </summary>
        /// <param name="x"></param>
        /// <param name="c"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private static BigInteger fX(BigInteger x, BigInteger c, BigInteger n)
        {
            return (Intermediaries.PowMod(x, 2, n) + c) % n;
        }

        /// <summary>
        /// 2x + d mod n
        /// </summary>
        /// <param name="x"></param>
        /// <param name="d"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private static BigInteger gx(BigInteger x, BigInteger d, BigInteger n)
        {
            return ((x << 1) + d) % n;
        }

        /// <summary>
        /// finds one Cyclic group generator, searchType specifies which algorithm you want to use
        /// </summary>
        /// <param name="modulus"></param>
        /// <param name="searchType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static BigInteger getGenerator(BigInteger modulus, GeneratorSearch searchType)
        {
            switch (searchType)
            {
                case GeneratorSearch.Brute:
                    return FindGenerator(modulus);
                case GeneratorSearch.PollardRho:
                    return pollardGenerator(modulus);
                case GeneratorSearch.IndexCalculator:
                    break;
                case GeneratorSearch.IndexCandidates:
                    break;
                default:
                    throw new ArgumentException();
            }



            return -1;
        }

        private static BigInteger pollardGenerator(BigInteger modulus)
        {
            BigInteger generator = Intermediaries.getRandomBigInteger(2, modulus - 1);
            BigInteger[] primes = pollardRho(eulerPhi(modulus,EulerPhiSelection.Mobius));
            Array.Sort(primes);

            for (int i = 1; i < primes.Length; i++)
            {
                if (Intermediaries.PowMod(generator, i, modulus) != 1)
                {
                    generator = Intermediaries.getRandomBigInteger(2, modulus - 1);
                    while ((BigInteger.GreatestCommonDivisor(generator, modulus) != 1))
                    {
                        generator = Intermediaries.getRandomBigInteger(2, modulus - 1);
                    }
                }
            }


            return generator;
        }

        public static BigInteger eulerPhi(BigInteger modulus, EulerPhiSelection selection)
        {
            switch (selection)
            {
                case EulerPhiSelection.Brute:
                    return eulerPhiBrute(modulus);
                case EulerPhiSelection.Mobius:
                    return eulerPhiMobius(modulus);
                case EulerPhiSelection.Classic:
                    break;
                default:
                    return -1;
            }
            return -1;
        }

        private static BigInteger eulerPhiBrute(BigInteger modulus)
        {
            BigInteger res = modulus - 1;

            for (BigInteger i = 2; i < modulus; i++)
            {
                if (BigInteger.GreatestCommonDivisor(i, modulus) != 1)
                {
                    res--;
                }
            }

            return res;
        }


        private static BigInteger eulerPhiMobius(BigInteger modulus)
        {
            BigInteger[] primes = pollardRho(modulus);
            if (modulus == primes[0])
            {
                return modulus - 1;
            }

            BigInteger result = 0;

            for (int i = 1; i <= modulus; i++)
            {
                if (modulus % i == 0)
                {
                    result += mobiusFunction(i) * modulus / i;
                    Console.WriteLine(mobiusFunction(i) + " " + i );
                }
            }
            return result;
        }

        private static BigInteger mobiusFunction(BigInteger n)
        {
            if (n == 1)
            {
                return 1;
            }

            BigInteger[] factors = pollardRho(n);


            if (!isWithoutSquares(factors))
            {
                return 0;
            }

            if (factors.Length % 2 == 1)
            {
                return -1;
            }

            return 1;
        }

        /// <summary>
        /// Möbius function
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static bool isWithoutSquares(BigInteger[] primeFactors)
        {
            List<BigInteger> list = new List<BigInteger>(primeFactors);
            var hashSet = new HashSet<BigInteger>(list);
            if (list.Count == hashSet.Count)
            {
                return true;
            }

            return false;
        }


        private struct Point
        {
            public BigInteger x;
            public BigInteger y;

            public static Point pointAddition(Point point, EllpticCurve curve)
            {
                Point result = new Point();

                BigInteger s = (3 * point.x * point.x + curve.a) *
                               Intermediaries.PoMod(point.y<<1, curve.modulo - 2, curve.modulo);
                

                result.x = (s*s - point.x - point.x) % curve.modulo;
                result.y = (s*(point.x  - result.x) - point.y) % curve.modulo;

                return result;
            }
        }

        private struct EllpticCurve
        {
            public BigInteger a, b, modulo;

        }

        private static BigInteger[] LECF(BigInteger modulus)
        {
            List<BigInteger> list = new List<BigInteger>();
            int numberOfIterations = 0;
            while (modulus !=1 )
            {
                list.Add(LenstraEllipticCurveFactorization(modulus));
                numberOfIterations++;
                if (list[list.Count-1] == -1)
                {
                    list.RemoveAt(list.Count-1);
                }
                else
                {
                    modulus /= list[list.Count - 1];
                }

                if (numberOfIterations > 500)
                {
                    list.Add(modulus);
                    modulus /= modulus;
                }
                

                
            }

            BigInteger[] result = new BigInteger[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                result[i] = (BigInteger)list[i];
            }
            return result;
        }


        private static BigInteger LenstraEllipticCurveFactorization(BigInteger modulus)
        {
            if (modulus % 2 == 0)
            {
                return 2;
            }else if (modulus % 3 == 0)
            {
                return 3;
            }else if (modulus % 5 == 0)
            {
                return 5;
            }else if (modulus % 7 == 0)
            {
                return 7;
            }else if (modulus % 11 == 0)
            {
                return 11;
            }



            Point P0 = new Point();
            P0.x = Intermediaries.getRandomBigInteger(1, modulus - 1);
            P0.y = Intermediaries.getRandomBigInteger(1, modulus - 1);

            EllpticCurve E = new EllpticCurve();
            E.a = Intermediaries.getRandomBigInteger(1, modulus - 1);

            //b = y^2-x0^3-a*x0
            E.b = Intermediaries.getRandomBigInteger(1, modulus - 1);
            //(P0.y * P0.y - P0.x * P0.x * P0.x - E.a * P0.x) % modulus;
            E.modulo = modulus;

            List<Point> points = new List<Point>();

            points.Add(P0);

            BigInteger d = 0;
            if (modulus > 10000000)
            {
                for (int i = 1; i < Intermediaries.SqrtFast(Intermediaries.SqrtFast(modulus)); i++)
                {
                    points.Add(Point.pointAddition(points[i - 1], E));

                    d = BigInteger.GreatestCommonDivisor(modulus, points[i - 1].y);

                    if (d > 1 && d < modulus)
                    {
                        return d;
                    }
                }
            }
            else
            {
                for (int i = 1; i < Intermediaries.SqrtFast(modulus); i++)
                {
                    points.Add(Point.pointAddition(points[i - 1], E));

                    d = BigInteger.GreatestCommonDivisor(modulus, points[i - 1].y);

                    if (d > 1 && d < modulus)
                    {
                        return d;
                    }
                }
            }
            
            
            return -1;
        }

        public static BigInteger[] Factorization(BigInteger n, FactorizationSelection selection)
        {
            switch (selection)
            {
                case FactorizationSelection.Brute:
                    return factorizationBrute(n);
                case FactorizationSelection.PollarRho:
                    return pollardRho(n);
                case FactorizationSelection.BabyStepGiantStep:
                    return new BigInteger[] { };
                case FactorizationSelection.LenstraEllipticCurveFactorization:
                    return LECF(n);
                default:
                    break;
            }
            return new BigInteger[] { };
        }

        private static BigInteger[] factorizationBrute(BigInteger n)
        {
            BigInteger[] result;
            List<BigInteger> factors = new List<BigInteger>();

            for (BigInteger i = 2; i < Intermediaries.SqrtFast(n); i++)
            {
                if (n % i == 0)
                {
                    n /= i;
                    factors.Add(i);
                    i--;
                }
            }

            result = new BigInteger[factors.Count];

            for (int i = 0; i < factors.Count; i++)
            {
                result[i] = factors[i];
            }

            return result;
        }

        private static BigInteger[] BSGSfactorization(BigInteger n)//baby step giant step
        {
            BigInteger x = Intermediaries.SqrtFast(n);
            BigInteger y = 0;

            List<BigInteger> list = new List<BigInteger>();

            while (true)
            {
                BigInteger z = x * x - n * y * y;

                if(z==1)
                {
                    return x;
                }
            }
        }
    }

    internal class Intermediaries
    {
        private static Random random = new Random();

        public static BigInteger Sqrt(BigInteger n)
        {
            if (n == 0) return 0;
            BigInteger x = n / 2 + 1;
            BigInteger y = (x + n / x) / 2;
            while (y < x)
            {
                x = y;
                y = (x + n / x) / 2;
            }
            return x;
        }


        /// <summary>
        /// Returns x^y mod p
        /// </summary>
        /// <param name="x">base</param>
        /// <param name="y">exponent</param>
        /// <param name="p">modulus</param>
        /// <returns></returns>
        public static BigInteger PowMod(BigInteger x, BigInteger y, BigInteger p)
        {
            if (y < (BigInteger)0)
            {
                y = p + y;
            }

            return (PMod((BigInteger)x, (BigInteger)y, (BigInteger)p));
        }

        public static BigInteger PoMod(BigInteger x, BigInteger y, BigInteger p)
        {
            if (y < (BigInteger)0)
            {
                y = p + y;
            }

            return (PMod((BigInteger)x, (BigInteger)y, (BigInteger)p));
        }

        private static BigInteger PMod(BigInteger x, BigInteger y, BigInteger p)
        {

            BigInteger res = 1;
            x = x % p;
            while (y > 0)
            {
                if ((y & 1) == 1)
                    res = (res * x) % p;
                y = y >> 1;
                x = (x * x) % p;
            }
            return res;
        }

        private static readonly BigInteger FastSqrtSmallNumber = 4503599761588223UL;

        public static BigInteger SqrtFast(BigInteger value)
        {
            if (value <= FastSqrtSmallNumber) // small enough for Math.Sqrt() or negative?
            {
                if (value.Sign < 0) throw new ArgumentException("Negative argument.");
                return (ulong)Math.Sqrt((ulong)value);
            }

            BigInteger root; // now filled with an approximate value
            int byteLen = value.ToByteArray().Length;
            if (byteLen < 128) // small enough for direct double conversion?
            {
                root = (BigInteger)Math.Sqrt((double)value);
            }
            else // large: reduce with bitshifting, then convert to double (and back)
            {
                root = (BigInteger)Math.Sqrt((double)(value >> (byteLen - 127) * 8)) << (byteLen - 127) * 4;
            }

            for (; ; )
            {
                var root2 = value / root + root >> 1;
                if ((root2 == root || root2 == root + 1) && IsSqrt(value, root)) return root;
                root = value / root2 + root2 >> 1;
                if ((root == root2 || root == root2 + 1) && IsSqrt(value, root2)) return root2;
            }
        }

        public static bool IsSqrt(BigInteger value, BigInteger root)
        {
            var lowerBound = root * root;

            return value >= lowerBound && value <= lowerBound + (root << 1);
        }


        public static BigInteger getRandomBigInteger(BigInteger min, BigInteger max)
        {
            byte[] bytes = max.ToByteArray();
            BigInteger result;

            do
            {
                random.NextBytes(bytes);
                bytes[bytes.Length - 1] &= 0x7F; //remove sign
                result = new BigInteger(bytes);
            } while (result < min || result > max);

            return result;
        }


    }
}
