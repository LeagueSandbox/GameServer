using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PacketDefinitions420;

namespace LeagueSandbox.GameServerTests.Tests.Blowfish
{
    [TestClass]
    public class TestBlowfish
    {
        [TestMethod]
        public void TestBlowfish1()
        {
            try
            {
                new BlowFish(new byte[60]);
                Assert.Fail("Should've failed");
            }
            catch { }

            var b = new BlowFish(Encoding.ASCII.GetBytes("myAwesomeKey"));
            var plainText = Encoding.Default.GetBytes("The quick brown fox jumped over the lazy dog.");
            var cipherText = b.Encrypt(plainText);
            CollectionAssert.AreNotEqual(plainText, cipherText);

            var decypherText = b.Decrypt(cipherText);
            CollectionAssert.AreEqual(plainText, decypherText);

            var encryptedLong = BitConverter.ToUInt64(b.Encrypt(BitConverter.GetBytes((ulong)12345)),0);
            var decryptedLong = b.Decrypt(encryptedLong);
            Assert.AreEqual(12345, decryptedLong);
        }
    }
}
