using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BlowFishCS;
using System.Text;

namespace GameServerTests
{
    [TestClass]
    public class TestBlowfish
    {
        [TestMethod]
        public void TestBlowfish1()
        {
            var b = new BlowFish(Encoding.ASCII.GetBytes("myAwesomeKey"));
            var plainText = Encoding.Default.GetBytes("The quick brown fox jumped over the lazy dog.");
            var cipherText = b.Encrypt(plainText);
            CollectionAssert.AreNotEqual(plainText, cipherText);

            var decypherText = b.Decrypt(cipherText);
            CollectionAssert.AreEqual(plainText, decypherText);
        }
    }
}
