using System;
using System.Collections.Generic;

namespace TheLoop.Game
{
    /// <summary>What a board cell is. Start/Goal are fixed; the rest is shapeable terrain.</summary>
    public enum CellType
    {
        Empty,
        Wall,
        Hazard,
        Reward,
        Start,
        Goal,
    }

    /// <summary>
    /// The run's tabletop world (XRC-101): a grid seeded deterministically from a run seed, with
    /// exactly one Start and one Goal. Plain C# and engine-free so the whole loop is unit-testable.
    /// </summary>
    public sealed class RunBoard
    {
        public const int DefaultSize = 8;

        private readonly CellType[,] _cells;

        public RunBoard(int width, int height)
        {
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException();
            Width = width;
            Height = height;
            _cells = new CellType[width, height];
        }

        public int Width { get; }
        public int Height { get; }
        public Coord Start { get; private set; }
        public Coord Goal { get; private set; }

        public CellType Get(Coord c) => _cells[c.X, c.Y];

        public void Set(Coord c, CellType type) => _cells[c.X, c.Y] = type;

        public bool InBounds(Coord c) => c.X >= 0 && c.X < Width && c.Y >= 0 && c.Y < Height;

        /// <summary>A cell the agent may stand on — in bounds and not a wall.</summary>
        public bool IsWalkable(Coord c) => InBounds(c) && Get(c) != CellType.Wall;

        /// <summary>The in-bounds 4-neighbours (no diagonals) of a cell.</summary>
        public IEnumerable<Coord> Neighbours(Coord c)
        {
            var candidates = new[]
            {
                new Coord(c.X + 1, c.Y),
                new Coord(c.X - 1, c.Y),
                new Coord(c.X, c.Y + 1),
                new Coord(c.X, c.Y - 1),
            };

            foreach (var n in candidates)
            {
                if (InBounds(n)) yield return n;
            }
        }

        public int Count(CellType type)
        {
            var n = 0;
            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                if (_cells[x, y] == type) n++;
            }

            return n;
        }

        /// <summary>
        /// Build a fresh board from a run seed: Start in the left band, Goal in the right band,
        /// everything else Empty. Deterministic — the same seed always yields the same board.
        /// </summary>
        public static RunBoard Seeded(int seed, int size = DefaultSize)
        {
            var board = new RunBoard(size, size);
            var rng = new Random(seed);

            var start = new Coord(rng.Next(0, Math.Max(1, size / 4)), rng.Next(0, size));

            Coord goal;
            do
            {
                goal = new Coord(size - 1 - rng.Next(0, Math.Max(1, size / 4)), rng.Next(0, size));
            }
            while (goal == start);

            board.Start = start;
            board.Goal = goal;
            board.Set(start, CellType.Start);
            board.Set(goal, CellType.Goal);
            return board;
        }
    }

    /// <summary>The autonomous pawn the player trains (XRC-101 holds only its state).</summary>
    public sealed class Agent
    {
        public const int DefaultHp = 3;

        public Agent(Coord position, int hp = DefaultHp)
        {
            Position = position;
            Hp = hp;
        }

        public Coord Position;
        public int Hp;

        public bool IsDown => Hp <= 0;
    }
}
