﻿using System.Collections.Generic;
using System.Text;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PhysicsLogic;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.SamplesFramework
{
    internal class AdvancedDemo5 : PhysicsGameScreen, IDemoScreen
    {
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Breakable bodies and explosions";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("TODO: Add sample description!");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Explode (at cursor): B button");
            sb.AppendLine("  - Move cursor: left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: left thumbstick");
            sb.AppendLine("  - Exit to menu: Back button");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Exit to menu: Escape");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse / Touchscreen");
            sb.AppendLine("  - Explode (at cursor): Right click");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.AppendLine("  - Drag grabbed object: move mouse / finger");
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = Vector2.Zero;

            new Border(World, ScreenManager.GraphicsDevice.Viewport);

            Texture2D alphabet = ScreenManager.Content.Load<Texture2D>("Samples/alphabet");

            uint[] data = new uint[alphabet.Width * alphabet.Height];
            alphabet.GetData(data);

            List<Vertices> list = PolygonTools.CreatePolygon(data, alphabet.Width, 3.5f, 20, true, true);

            float yOffset = 0f;
            float xOffset = -26.25f;
            for (int i = 0; i < list.Count; i++)
            {
                xOffset += 3.5f;

                if (i == 14)
                {
                    yOffset = 5f;
                    xOffset = -19.25f;
                }

                Vertices polygon = list[i];
                Vector2 centroid = -polygon.GetCentroid();
                polygon.Translate(ref centroid);
                polygon = SimplifyTools.CollinearSimplify(polygon);
                polygon = SimplifyTools.ReduceByDistance(polygon, 4);
                List<Vertices> triangulated = BayazitDecomposer.ConvexPartition(polygon);

#if WINDOWS_PHONE
                float _scale = 0.6f;
#else
                float _scale = 1f;
#endif
                Vector2 vertScale = new Vector2(ConvertUnits.ToSimUnits(1)) * _scale;
                foreach (Vertices vertices in triangulated)
                {
                    vertices.Scale(ref vertScale);
                }

                BreakableBody breakableBody = new BreakableBody(triangulated, World, 1);
                breakableBody.MainBody.Position = new Vector2(xOffset, yOffset);
                breakableBody.Strength = 100;
                World.AddBreakableBody(breakableBody);
            }
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.IsNewMouseButtonPress(MouseButtons.RightButton) ||
                input.IsNewButtonPress(Buttons.B))
            {
                Vector2 cursorPos = Camera.ConvertScreenToWorld(input.Cursor);

                Vector2 min = cursorPos - new Vector2(10, 10);
                Vector2 max = cursorPos + new Vector2(10, 10);

                AABB aabb = new AABB(ref min, ref max);

                World.QueryAABB(fixture =>
                                {
                                    Vector2 fv = fixture.Body.Position - cursorPos;
                                    fv.Normalize();
                                    fv *= 40;
                                    fixture.Body.ApplyLinearImpulse(ref fv);
                                    return true;
                                }, ref aabb);
            }

            base.HandleInput(input, gameTime);
        }
    }
}