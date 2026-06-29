using System.Collections.Generic;
using TheLoop.Game;
using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    /// <summary>
    /// The playable Session board (XRC-104). Renders the live <see cref="RunDriver"/>'s grid —
    /// terrain, hazards, rewards, walls, start/goal, the agent and its intended next step — and
    /// shows the agent's one-line reason (the transparent-agent pillar). A brush palette lets the
    /// player tap a cell to shape the run while the agent moves on its own. The screen polls the
    /// run state while shown; the Session state advances the driver on its cadence.
    /// </summary>
    public sealed class SessionScreen : ScreenBase
    {
        private static readonly string[] CellClasses =
        {
            "cell-wall", "cell-hazard", "cell-reward", "cell-start", "cell-goal", "cell-intent",
        };

        private Label _phase;
        private Label _hp;
        private Label _score;
        private Label _intent;
        private VisualElement _boardHost;
        private VisualElement[,] _cells;
        private VisualElement[,] _pips;

        private readonly List<Button> _brushes = new List<Button>();
        private CellType _brush = CellType.Hazard;
        private IVisualElementScheduledItem _poll;
        private int _w;
        private int _h;

        public override GameState State => GameState.Session;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            panel.AddToClassList("panel--wide");
            panel.Add(Ui.Eyebrow("THE RUN"));
            panel.Add(Ui.Heading("Session"));

            _phase = new Label();
            _phase.AddToClassList("badge");
            panel.Add(_phase);

            var stat = new VisualElement();
            stat.AddToClassList("button-row");
            _hp = Ui.Body("HP —");
            _score = Ui.Body("Score —");
            stat.Add(_hp);
            stat.Add(_score);
            panel.Add(stat);

            _intent = Ui.Caption(string.Empty);
            panel.Add(_intent);

            _boardHost = new VisualElement();
            _boardHost.AddToClassList("board");
            panel.Add(_boardHost);

            var palette = new VisualElement();
            palette.AddToClassList("button-row");
            palette.Add(Brush("Hazard", CellType.Hazard));
            palette.Add(Brush("Reward", CellType.Reward));
            palette.Add(Brush("Wall", CellType.Wall));
            panel.Add(palette);

            panel.Add(Ui.Caption("Tap a cell to place a tile. The agent runs on its own — shape its path."));

            root.Add(panel);
            return root;
        }

        public override void Bind() => UpdateBrushes();

        protected override void OnShow()
        {
            UpdateBrushes();
            _poll = Root.schedule.Execute(Refresh).Every(100);
        }

        protected override void OnHide()
        {
            _poll?.Pause();
            _poll = null;
        }

        private Button Brush(string label, CellType type)
        {
            var b = Ui.MenuButton(label, () =>
            {
                _brush = type;
                UpdateBrushes();
            });
            b.userData = type;
            _brushes.Add(b);
            return b;
        }

        private void UpdateBrushes()
        {
            foreach (var b in _brushes)
            {
                if ((CellType)b.userData == _brush) b.AddToClassList("brush--selected");
                else b.RemoveFromClassList("brush--selected");
            }
        }

        private RunDriver Driver => Context.Services.TryGet<RunService>(out var rs) ? rs.Current : null;

        private void Place(Coord c)
        {
            Driver?.TryPlace(c, _brush);
            Refresh();
        }

        private void EnsureBoard(RunBoard board)
        {
            if (_cells != null && _w == board.Width && _h == board.Height) return;

            _w = board.Width;
            _h = board.Height;
            _boardHost.Clear();
            _cells = new VisualElement[_w, _h];
            _pips = new VisualElement[_w, _h];

            for (var y = _h - 1; y >= 0; y--) // higher rows on top
            {
                var row = new VisualElement();
                row.AddToClassList("board-row");
                for (var x = 0; x < _w; x++)
                {
                    var coord = new Coord(x, y);
                    var cell = new VisualElement();
                    cell.AddToClassList("board-cell");

                    var pip = new VisualElement();
                    pip.AddToClassList("agent-pip");
                    cell.Add(pip);

                    cell.RegisterCallback<ClickEvent>(_ => Place(coord));

                    _cells[x, y] = cell;
                    _pips[x, y] = pip;
                    row.Add(cell);
                }

                _boardHost.Add(row);
            }
        }

        private void Refresh()
        {
            var d = Driver;
            if (d == null) return;

            EnsureBoard(d.Board);

            for (var x = 0; x < _w; x++)
            for (var y = 0; y < _h; y++)
            {
                var cell = _cells[x, y];
                foreach (var c in CellClasses) cell.RemoveFromClassList(c);

                switch (d.Board.Get(new Coord(x, y)))
                {
                    case CellType.Wall: cell.AddToClassList("cell-wall"); break;
                    case CellType.Hazard: cell.AddToClassList("cell-hazard"); break;
                    case CellType.Reward: cell.AddToClassList("cell-reward"); break;
                    case CellType.Start: cell.AddToClassList("cell-start"); break;
                    case CellType.Goal: cell.AddToClassList("cell-goal"); break;
                }

                if (d.Agent.Position == new Coord(x, y)) _pips[x, y].AddToClassList("agent-pip--on");
                else _pips[x, y].RemoveFromClassList("agent-pip--on");
            }

            if (!d.IsComplete && d.Board.InBounds(d.Plan.Next))
            {
                _cells[d.Plan.Next.X, d.Plan.Next.Y].AddToClassList("cell-intent");
            }

            _hp.text = $"HP {d.Agent.Hp}";
            _score.text = $"Score {d.Score}";
            _phase.text = d.IsComplete ? "RESOLVING" : "AGENT RUNNING";
            _intent.text = d.IsComplete ? "Run complete." : d.Plan.Reason;
        }
    }
}
