using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TheLoop.Game
{
    /// <summary>
    /// The agent's "trained" state (XRC-102): per-cell danger and reward weights that persist
    /// across runs. Cells where it was hurt accrue lasting aversion; reward cells accrue mild
    /// attraction. Memory decays slowly so the agent stays adaptable. Serializes to a compact
    /// string so it can ride on the save profile (XRC-92). Engine-free and unit-testable.
    /// </summary>
    public sealed class AgentMemory
    {
        private const float Epsilon = 0.05f;

        private readonly Dictionary<Coord, float> _danger = new Dictionary<Coord, float>();
        private readonly Dictionary<Coord, float> _reward = new Dictionary<Coord, float>();

        public float Danger(Coord c) => _danger.TryGetValue(c, out var v) ? v : 0f;
        public float Reward(Coord c) => _reward.TryGetValue(c, out var v) ? v : 0f;

        public void RememberDanger(Coord c, float amount) => _danger[c] = Danger(c) + amount;
        public void RememberReward(Coord c, float amount) => _reward[c] = Reward(c) + amount;

        /// <summary>Slowly fade weights toward zero (call once per run) so the agent stays adaptable.</summary>
        public void Decay(float factor)
        {
            Fade(_danger, factor);
            Fade(_reward, factor);
        }

        private static void Fade(Dictionary<Coord, float> map, float factor)
        {
            var drop = new List<Coord>();
            var keys = new List<Coord>(map.Keys);
            foreach (var k in keys)
            {
                var v = map[k] * factor;
                if (v < Epsilon) drop.Add(k);
                else map[k] = v;
            }

            foreach (var k in drop) map.Remove(k);
        }

        /// <summary>Compact "x,y,danger,reward" entries joined by ';'. Invariant-culture floats.</summary>
        public string Serialize()
        {
            var keys = new HashSet<Coord>(_danger.Keys);
            keys.UnionWith(_reward.Keys);

            var sb = new StringBuilder();
            foreach (var c in keys)
            {
                if (sb.Length > 0) sb.Append(';');
                sb.Append(c.X).Append(',').Append(c.Y).Append(',')
                    .Append(Danger(c).ToString("0.##", CultureInfo.InvariantCulture)).Append(',')
                    .Append(Reward(c).ToString("0.##", CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }

        /// <summary>Parse a serialized memory. Corruption-tolerant — bad entries are skipped.</summary>
        public static AgentMemory Deserialize(string data)
        {
            var memory = new AgentMemory();
            if (string.IsNullOrEmpty(data)) return memory;

            foreach (var entry in data.Split(';'))
            {
                var parts = entry.Split(',');
                if (parts.Length != 4) continue;
                if (!int.TryParse(parts[0], out var x)) continue;
                if (!int.TryParse(parts[1], out var y)) continue;
                if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) continue;
                if (!float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var r)) continue;

                var c = new Coord(x, y);
                if (d != 0f) memory._danger[c] = d;
                if (r != 0f) memory._reward[c] = r;
            }

            return memory;
        }
    }
}
