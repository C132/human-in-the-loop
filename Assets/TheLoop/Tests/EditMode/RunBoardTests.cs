using NUnit.Framework;
using TheLoop.Game;

namespace TheLoop.Tests.EditMode
{
    /// <summary>XRC-101: deterministic board seeding + grid helpers.</summary>
    public sealed class RunBoardTests
    {
        [Test]
        public void Seeded_IsDeterministic()
        {
            var a = RunBoard.Seeded(1234);
            var b = RunBoard.Seeded(1234);

            Assert.AreEqual(a.Start, b.Start);
            Assert.AreEqual(a.Goal, b.Goal);
            for (var x = 0; x < a.Width; x++)
            for (var y = 0; y < a.Height; y++)
            {
                Assert.AreEqual(a.Get(new Coord(x, y)), b.Get(new Coord(x, y)));
            }
        }

        [Test]
        public void Seeded_HasExactlyOneStartAndGoal_NotEqual()
        {
            var board = RunBoard.Seeded(42);

            Assert.AreEqual(1, board.Count(CellType.Start));
            Assert.AreEqual(1, board.Count(CellType.Goal));
            Assert.AreNotEqual(board.Start, board.Goal);
            Assert.AreEqual(CellType.Start, board.Get(board.Start));
            Assert.AreEqual(CellType.Goal, board.Get(board.Goal));
            Assert.IsTrue(board.InBounds(board.Start) && board.InBounds(board.Goal));
        }

        [Test]
        public void Seeded_StartLeft_GoalRight()
        {
            // Across many seeds, Start sits in the left band and Goal in the right band.
            for (var seed = 0; seed < 50; seed++)
            {
                var b = RunBoard.Seeded(seed);
                Assert.Less(b.Start.X, b.Width / 2, "start is on the left");
                Assert.GreaterOrEqual(b.Goal.X, b.Width / 2, "goal is on the right");
            }
        }

        [Test]
        public void InBounds_Walkable_Neighbours()
        {
            var board = RunBoard.Seeded(7);

            Assert.IsTrue(board.InBounds(new Coord(0, 0)));
            Assert.IsFalse(board.InBounds(new Coord(-1, 0)));
            Assert.IsFalse(board.InBounds(new Coord(board.Width, 0)));

            var wall = new Coord(3, 3);
            board.Set(wall, CellType.Wall);
            Assert.IsFalse(board.IsWalkable(wall));
            Assert.IsTrue(board.IsWalkable(new Coord(4, 4)));

            // Corner has 2 neighbours; an interior cell has 4.
            Assert.AreEqual(2, CountNeighbours(board, new Coord(0, 0)));
            Assert.AreEqual(4, CountNeighbours(board, new Coord(4, 4)));
        }

        [Test]
        public void Coord_EqualityAndManhattan()
        {
            Assert.AreEqual(new Coord(2, 3), new Coord(2, 3));
            Assert.AreNotEqual(new Coord(2, 3), new Coord(3, 2));
            Assert.AreEqual(5, new Coord(0, 0).ManhattanTo(new Coord(2, 3)));
        }

        private static int CountNeighbours(RunBoard board, Coord c)
        {
            var n = 0;
            foreach (var _ in board.Neighbours(c)) n++;
            return n;
        }
    }
}
