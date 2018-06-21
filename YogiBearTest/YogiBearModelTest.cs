using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using YogiBear.Model;
using YogiBear.Persistence;

namespace YogiBearTest
{
    [TestClass]
    public class YogiBearModelTest
    {
        private YogiBearModel model;
        private YogiBearData data;
        private String level1;

        [TestInitialize]
        public void Initialize()
        {
            model = new YogiBearModel();
            data = new YogiBearData();
            level1 = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\level1.map";
        }

        [TestMethod]
        public void YogiBearModelCtorTest()
        {
            Assert.AreEqual(model.Map.Count, 0);
            Assert.AreEqual(model.Rangers.Count, 0);
            Assert.AreEqual(model.PlayerPos.X, 0);
            Assert.AreEqual(model.PlayerPos.Y, 0);
            Assert.AreEqual(model.Baskets, 0);
        }

        [TestMethod]
        public void YogiBearModelNewGameTest()
        {
            model.Map = data.LoadFirstLevel(level1);
            Assert.AreEqual(model.Map.Count, 8);
            for (Int32 i = 0; i < 8; i++)
                Assert.AreEqual(model.Map[i].Count, 8);

            for (Int32 i = 0; i < 8; i++)
                for (Int32 j = 0; j < 8; j++)
                    Assert.IsTrue(model.Map[i][j] > -1 && model.Map[i][j] < 5);
        }

        [TestMethod]
        public void YogiBearModelIsFloorTest()
        {
            model.Map = data.LoadFirstLevel(level1);
            Assert.IsFalse(model.IsFloor(10, 1));
            for (Int32 i = 0; i < 8; i++)
                for (Int32 j = 0; j < 8; j++)
                {
                    if(model.Map[i][j] == 0 || model.Map[i][j] == 2 || model.Map[i][j] == 3)
                        Assert.IsTrue(model.IsFloor(i, j));
                    else
                        Assert.IsFalse(model.IsFloor(i, j));
                }
        }

        [TestMethod]
        public void YogiBearModelStepsTest()
        {
            model.Map = data.LoadFirstLevel(level1);

            Assert.AreEqual(model.PlayerPos.X, 0);
            Assert.AreEqual(model.PlayerPos.Y, 0);
            model.Down();
            Assert.AreEqual(model.PlayerPos.X, 1);
            model.Up();
            Assert.AreEqual(model.PlayerPos.X, 0);
            model.Right();
            Assert.AreEqual(model.PlayerPos.Y, 1);
            model.Left();
            Assert.AreEqual(model.PlayerPos.X, 0);
        }
    }
}
