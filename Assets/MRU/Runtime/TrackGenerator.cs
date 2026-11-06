using System;
using System.Collections.Generic;
using UnityEngine;

namespace MRU.InfiniteRunner
{
    public enum PickupKind { Coin, Magnet, Shield, DoubleCoins, Multiplier }

    public sealed class TrackGenerator
    {
        private const float SegmentLength = 32f;
        private const int SegmentCount = 10;
        private readonly GameManager game;
        private readonly List<Segment> segments = new List<Segment>();
        private readonly WorldStyle[] worlds =
        {
            new WorldStyle("Ancient Temple",       new Color(0.55f, 0.42f, 0.25f), new Color(0.85f, 0.65f, 0.25f), new Color(0.60f, 0.78f, 0.98f), "Stone Obelisks & Ruins"),
            new WorldStyle("Jungle Adventure",     new Color(0.22f, 0.42f, 0.18f), new Color(0.12f, 0.75f, 0.28f), new Color(0.48f, 0.78f, 0.58f), "Canopy Palms & Ancient Vines"),
            new WorldStyle("Sky Kingdom",          new Color(0.75f, 0.82f, 0.92f), new Color(0.35f, 0.75f, 1.00f), new Color(0.45f, 0.75f, 1.00f), "Cloud Pillars & Floating Shards"),
            new WorldStyle("Desert Adventure",     new Color(0.82f, 0.62f, 0.32f), new Color(0.95f, 0.45f, 0.12f), new Color(0.98f, 0.78f, 0.48f), "Dunes, Pyramids & Cacti"),
            new WorldStyle("Snow Mountain",        new Color(0.78f, 0.88f, 0.95f), new Color(0.25f, 0.72f, 0.95f), new Color(0.62f, 0.82f, 0.98f), "Frost Glaciers & Snowy Pines"),
            new WorldStyle("Volcano",              new Color(0.20f, 0.18f, 0.18f), new Color(0.98f, 0.25f, 0.05f), new Color(0.28f, 0.10f, 0.12f), "Basalt Pillars & Lava Streams"),
            new WorldStyle("Futuristic Neon City", new Color(0.10f, 0.12f, 0.22f), new Color(0.95f, 0.05f, 0.85f), new Color(0.08f, 0.06f, 0.20f), "Cyber Skyscrapers & Holograms"),
            new WorldStyle("Alien Planet",         new Color(0.28f, 0.18f, 0.38f), new Color(0.75f, 0.15f, 0.95f), new Color(0.20f, 0.08f, 0.35f), "Glowing Crystals & Bio-Nodes"),
            new WorldStyle("Underwater Kingdom",   new Color(0.08f, 0.32f, 0.48f), new Color(0.05f, 0.85f, 0.75f), new Color(0.02f, 0.22f, 0.45f), "Coral Arches & Bioluminescence"),
            new WorldStyle("Space Station",        new Color(0.18f, 0.22f, 0.28f), new Color(0.05f, 0.75f, 0.98f), new Color(0.02f, 0.03f, 0.08f), "Orbital Modules & Starlight")
        };

        private readonly Material[] materials;
        private readonly System.Random random = new System.Random();
        private float furthestZ;

        public int CurrentWorld { get; private set; }
        public string WorldName => worlds[CurrentWorld].name;
        public string WorldDescription => worlds[CurrentWorld].description;

        public TrackGenerator(GameManager owner, int startingWorld)
        {
            game = owner;
            CurrentWorld = Mathf.Clamp(startingWorld, 0, worlds.Length - 1);
            materials = new Material[worlds.Length * 8];

            for (int i = 0; i < SegmentCount; i++)
            {
                Segment segment = new Segment(this);
                segments.Add(segment);
            }
            Reset(startingWorld);
        }

        public void Reset(int startingWorld)
        {
            CurrentWorld = Mathf.Clamp(startingWorld, 0, worlds.Length - 1);
            ApplyEnvironmentLighting();

            furthestZ = -15f + (SegmentCount - 1) * SegmentLength;
            for (int i = 0; i < segments.Count; i++)
            {
                segments[i].Root.position = new Vector3(0f, 0f, -15f + i * SegmentLength);
                ResetSegment(segments[i]);
            }
        }

        private void ApplyEnvironmentLighting()
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.backgroundColor = worlds[CurrentWorld].sky;
            }
            RenderSettings.fogColor = worlds[CurrentWorld].sky;
            RenderSettings.ambientLight = Color.Lerp(worlds[CurrentWorld].ground, Color.white, 0.4f);
        }

        public void Tick(float speed, float dt)
        {
            int requestedWorld = (game.StartWorld + game.DistanceMeters / 300) % worlds.Length;
            if (requestedWorld != CurrentWorld)
            {
                CurrentWorld = requestedWorld;
                ApplyEnvironmentLighting();
                foreach (Segment segment in segments) ApplyStyle(segment);
            }

            for (int i = 0; i < segments.Count; i++)
            {
                Segment segment = segments[i];
                segment.Root.position += Vector3.back * speed * dt;

                // Animate rotating coins & glowing powerups inside active segments
                segment.AnimatePickups(dt);

                if (segment.Root.position.z < -32f)
                {
                    segment.Root.position = new Vector3(0f, 0f, furthestZ + SegmentLength);
                    furthestZ = segment.Root.position.z;
                    ResetSegment(segment);
                }
                CheckObjects(segment);
            }
        }

        private void CheckObjects(Segment segment)
        {
            foreach (TrackedObject item in segment.Objects)
            {
                if (!item.Object.activeSelf) continue;
                Vector3 worldPos = item.Object.transform.position;

                if (!item.IsHazard)
                {
                    // Magnet pickup suction
                    bool isMagnetized = item.Kind == PickupKind.Coin && game.MagnetActive && Mathf.Abs(worldPos.z) < 14f;
                    if (isMagnetized)
                    {
                        Vector3 runnerPos = game.Runner.transform.position;
                        item.Object.transform.position = Vector3.MoveTowards(worldPos, runnerPos + Vector3.up * 1f, Time.deltaTime * 22f);
                        worldPos = item.Object.transform.position;
                    }

                    if ((Mathf.Abs(worldPos.z) < 0.95f && Mathf.Abs(worldPos.x - game.Runner.X) < 0.95f) || (isMagnetized && Mathf.Abs(worldPos.z) < 0.8f))
                    {
                        item.Object.SetActive(false);
                        game.Collect(item.Kind);
                    }
                }
                else if (item.IsHazard && game.Runner.Hits(item.Hazard, worldPos))
                {
                    item.Object.SetActive(false);
                    game.HitHazard();
                }
            }
        }

        private void ResetSegment(Segment segment)
        {
            foreach (TrackedObject item in segment.Objects) item.Object.SetActive(false);

            int hazardCount = random.NextDouble() < 0.35 ? 2 : 1;
            int firstLane = random.Next(0, 3);
            int secondLane = (firstLane + 1 + random.Next(0, 2)) % 3;
            float zOffset = -7f + (float)random.NextDouble() * 14f;

            ActivateHazard(segment, 0, firstLane, zOffset);
            if (hazardCount == 2) ActivateHazard(segment, 1, secondLane, zOffset + 2.5f);

            int safeLane = hazardCount == 1 ? (firstLane + 1 + random.Next(0, 2)) % 3 : (3 - firstLane - secondLane);

            // Spawn Coin Arc
            for (int i = 0; i < 10; i++)
            {
                TrackedObject coin = segment.Objects[segment.HazardSlots + i];
                float height = Mathf.Sin((i / 9f) * Mathf.PI) * 1.4f + 0.9f;
                coin.Object.transform.localPosition = new Vector3((safeLane - 1) * RunnerController.LaneWidth, height, -12f + i * 2.4f);
                coin.Object.SetActive(i < 8);
            }

            // Spawn Powerup with 30% chance
            if (random.NextDouble() < 0.30)
            {
                TrackedObject power = segment.Objects[segment.HazardSlots + 10];
                power.Kind = (PickupKind)random.Next((int)PickupKind.Magnet, (int)PickupKind.Multiplier + 1);
                power.Object.transform.localPosition = new Vector3((safeLane - 1) * RunnerController.LaneWidth, 1.35f, 10.5f);
                power.Object.SetActive(true);
            }

            ApplyStyle(segment);
        }

        private void ActivateHazard(Segment segment, int slot, int lane, float z)
        {
            TrackedObject item = segment.Objects[slot];
            item.IsHazard = true;
            item.Hazard = (HazardKind)random.Next(0, 3);

            float x = (lane - 1) * RunnerController.LaneWidth;
            if (item.Hazard == HazardKind.Slide)
            {
                item.Object.transform.localPosition = new Vector3(x, 1.85f, z);
                item.Object.transform.localScale = new Vector3(1.85f, 0.45f, 0.65f); // High arch barrier
            }
            else if (item.Hazard == HazardKind.Jump)
            {
                item.Object.transform.localPosition = new Vector3(x, 0.45f, z);
                item.Object.transform.localScale = new Vector3(1.85f, 0.75f, 0.65f); // Low barrier
            }
            else
            {
                item.Object.transform.localPosition = new Vector3(x, 1.1f, z);
                item.Object.transform.localScale = new Vector3(1.4f, 2.1f, 1.1f); // Solid block
            }
            item.Object.SetActive(true);
        }

        private void ApplyStyle(Segment segment)
        {
            WorldStyle style = worlds[CurrentWorld];
            segment.Floor.GetComponent<Renderer>().sharedMaterial = GetMaterial(CurrentWorld, 0, style.ground);
            segment.LeftWall.GetComponent<Renderer>().sharedMaterial = GetMaterial(CurrentWorld, 1, style.accent);
            segment.RightWall.GetComponent<Renderer>().sharedMaterial = GetMaterial(CurrentWorld, 1, style.accent);

            foreach (TrackedObject item in segment.Objects)
            {
                if (!item.Object.activeSelf) continue;

                Renderer rend = item.Object.GetComponent<Renderer>();
                if (rend == null) continue;

                if (item.IsHazard)
                {
                    rend.sharedMaterial = GetMaterial(CurrentWorld, 1, style.accent);
                }
                else if (item.Kind == PickupKind.Coin)
                {
                    rend.sharedMaterial = GetMaterial(CurrentWorld, 2, new Color(1f, 0.82f, 0.08f));
                }
                else
                {
                    rend.sharedMaterial = GetMaterial(CurrentWorld, PickupSlot(item.Kind), PickupColor(item.Kind));
                }
            }

            segment.LeftProp.GetComponent<Renderer>().sharedMaterial = GetMaterial(CurrentWorld, 1, style.accent);
            segment.RightProp.GetComponent<Renderer>().sharedMaterial = GetMaterial(CurrentWorld, 1, style.accent);
        }

        private Material GetMaterial(int world, int slot, Color color)
        {
            int index = world * 8 + slot;
            if (materials[index] == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                Material mat = new Material(shader) { color = color };
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.65f);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.3f);
                materials[index] = mat;
            }
            return materials[index];
        }

        private static Color PickupColor(PickupKind kind)
        {
            switch (kind)
            {
                case PickupKind.Magnet: return new Color(0.05f, 0.95f, 1f);
                case PickupKind.Shield: return new Color(0.1f, 0.95f, 0.35f);
                case PickupKind.DoubleCoins: return new Color(1f, 0.15f, 0.85f);
                default: return new Color(1f, 0.55f, 0.05f); // Multiplier
            }
        }

        private static int PickupSlot(PickupKind kind)
        {
            switch (kind)
            {
                case PickupKind.Magnet: return 3;
                case PickupKind.Shield: return 4;
                case PickupKind.DoubleCoins: return 5;
                default: return 6;
            }
        }

        public sealed class WorldStyle
        {
            public readonly string name;
            public readonly Color ground, accent, sky;
            public readonly string description;

            public WorldStyle(string worldName, Color track, Color worldAccent, Color background, string desc)
            {
                name = worldName; ground = track; accent = worldAccent; sky = background; description = desc;
            }
        }

        public sealed class Segment
        {
            public readonly GameObject Root;
            public readonly GameObject Floor, LeftWall, RightWall, LeftProp, RightProp;
            public readonly List<TrackedObject> Objects = new List<TrackedObject>();
            public int HazardSlots = 3;

            public Segment(TrackGenerator owner)
            {
                Root = new GameObject("Pooled Track Segment");
                Floor = Make("Track Floor", PrimitiveType.Cube, Root.transform, new Vector3(0f, -0.3f, 0f), new Vector3(8.8f, 0.6f, SegmentLength));
                LeftWall = Make("Left Edge", PrimitiveType.Cube, Root.transform, new Vector3(-4.4f, 0.3f, 0f), new Vector3(0.5f, 1.2f, SegmentLength));
                RightWall = Make("Right Edge", PrimitiveType.Cube, Root.transform, new Vector3(4.4f, 0.3f, 0f), new Vector3(0.5f, 1.2f, SegmentLength));

                // World Landmarks
                LeftProp = Make("Left Pillar", PrimitiveType.Cylinder, Root.transform, new Vector3(-6.2f, 2.5f, -6f), new Vector3(1.4f, 5f, 1.4f));
                RightProp = Make("Right Pillar", PrimitiveType.Cylinder, Root.transform, new Vector3(6.2f, 2.5f, 6f), new Vector3(1.4f, 5f, 1.4f));

                // Hazard Pool
                for (int i = 0; i < HazardSlots; i++)
                    Objects.Add(new TrackedObject(Make("Hazard", PrimitiveType.Cube, Root.transform, Vector3.zero, Vector3.one), true));

                // Coin Pool
                for (int i = 0; i < 10; i++)
                    Objects.Add(new TrackedObject(Make("Coin", PrimitiveType.Cylinder, Root.transform, Vector3.zero, new Vector3(0.55f, 0.08f, 0.55f)), false));

                // Powerup Pool
                Objects.Add(new TrackedObject(Make("Powerup Orb", PrimitiveType.Sphere, Root.transform, Vector3.zero, new Vector3(0.9f, 0.9f, 0.9f)), false));
            }

            public void AnimatePickups(float dt)
            {
                float rot = 180f * dt;
                foreach (TrackedObject item in Objects)
                {
                    if (item.Object.activeSelf && !item.IsHazard)
                    {
                        item.Object.transform.Rotate(Vector3.up, rot, Space.World);
                    }
                }
            }

            private static GameObject Make(string name, PrimitiveType shape, Transform parent, Vector3 pos, Vector3 scale)
            {
                GameObject item = GameObject.CreatePrimitive(shape);
                item.name = name;
                item.transform.SetParent(parent, false);
                item.transform.localPosition = pos;
                item.transform.localScale = scale;
                Collider collider = item.GetComponent<Collider>();
                if (collider != null) UnityEngine.Object.Destroy(collider);
                return item;
            }
        }

        private sealed class TrackedObject
        {
            public readonly GameObject Object;
            public readonly bool IsHazard;
            public HazardKind Hazard;
            public PickupKind Kind;

            public TrackedObject(GameObject gameObject, bool hazard)
            {
                Object = gameObject; IsHazard = hazard;
                Kind = PickupKind.Coin;
                Object.SetActive(false);
            }
        }
    }
}
