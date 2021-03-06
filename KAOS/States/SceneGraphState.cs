﻿using KAOS.Interfaces;
using KAOS.Managers;
using KAOS.Nodes;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAOS.States
{
    public class SceneGraphState : AbstractState
    {
        private Node m_sceneGraph;
        private GroupNode m_hook1, m_hook2;

        private const float m_rotationspeed = 180.0f;
        private float m_spinangle;
        private float m_elapsedTime;

        public SceneGraphState(StateManager stateManager)
        {
            StateManager = stateManager;
            CreateSceneGraph();
            GL.Enable(EnableCap.DepthTest);
        }

        private void CreateSceneGraph()
        {
            PolygonNode poly1 = new PolygonNode();
            PolygonNode poly2 = new PolygonNode();
            PolygonNode poly3 = new PolygonNode();
            PolygonNode poly4 = new PolygonNode();
            GroupNode rt = new GroupNode();

            Vector3 a = new Vector3(.0f, .0f, 2.5f);
            Vector3 b = new Vector3(2.5f, .0f, -2.5f);
            Vector3 c = new Vector3(-2.5f, .0f, 2.5f);
            Vector3 d = new Vector3(.0f, 4.0f, .0f);

            poly1.AddNormal(new Vector3(.0f, -1.0f, .0f));
            poly1.AddVertex(0, c);
            poly1.AddVertex(1, b);
            poly1.AddVertex(2, c);

            poly2.AddNormal(new Vector3(.861411f, .269191f, .430706f));
            poly2.AddVertex(0, d);
            poly2.AddVertex(1, a);
            poly2.AddVertex(2, b);

            poly3.AddNormal(new Vector3(.0f, .529999f, -.847998f));
            poly3.AddVertex(0, d);
            poly3.AddVertex(1, b);
            poly3.AddVertex(2, c);

            poly4.AddNormal(new Vector3(-.861411f, .269191f, .430706f));
            poly4.AddVertex(0, d);
            poly4.AddVertex(1, c);
            poly4.AddVertex(2, a);

            GroupNode root = new GroupNode();
            GraphLinesNode graph = new GraphLinesNode();
            GroupNode rt1 = new GroupNode();
            GroupNode rt2 = new GroupNode();

            root.AddChild(graph);
            root.AddChild(rt1);
            root.AddChild(rt2);

            rt1.AddChild(rt);
            rt2.AddChild(rt);

            rt1.SetTranslation(5, 0, 0);
            rt2.SetTranslation(-5, 0, 0);

            rt.AddChild(poly1);
            rt.AddChild(poly2);
            rt.AddChild(poly3);
            rt.AddChild(poly4);

            m_sceneGraph = root;

            m_hook1 = rt1;
            m_hook2 = rt2;
        }

        public override void Update(float elapsedTime, float aspect)
        {
            m_elapsedTime = elapsedTime;
        }

        public override void Render()
        {
            m_spinangle += m_rotationspeed * m_elapsedTime;
            if (m_spinangle > 360)
            {
                m_spinangle = 0.0f;
            }

            Matrix4 lookat = Matrix4.LookAt(0, 20, 20, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);

            m_hook1.SetRotation(m_spinangle, 0, 1, 0);
            m_hook2.SetRotation(-m_spinangle, 0, 0, 1);

            m_sceneGraph.Render();
        }
    }
}
