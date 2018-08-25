using LeagueSandbox.GameServer.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LeagueSandbox.GameServerTests.Tests.API
{
    [TestClass]
    public class ApiFunctionManagerTests
    {
        [TestMethod]
        public void StringToByteArrayTest()
        {
            byte[] shouldBe = { 0xC6, 0x00, 0x10, 0x45, 0xF8};
            var byteArray = ApiFunctionManager.StringToByteArray("C6001045F8");
            CollectionAssert.AreEqual(shouldBe, byteArray);

            byteArray = ApiFunctionManager.StringToByteArray("C6 00 10 45 F8");
            CollectionAssert.AreEqual(shouldBe, byteArray);

            byteArray = ApiFunctionManager.StringToByteArray("C60010       45F8");
            CollectionAssert.AreEqual(shouldBe, byteArray);

            byteArray = ApiFunctionManager.StringToByteArray("C6001045F8      ");
            CollectionAssert.AreEqual(shouldBe, byteArray);

            byteArray = ApiFunctionManager.StringToByteArray("    C6001045F8");
            CollectionAssert.AreEqual(shouldBe, byteArray);

            byteArray = ApiFunctionManager.StringToByteArray("C600   1045  F8");
            CollectionAssert.AreEqual(shouldBe, byteArray);
        }
    }
}