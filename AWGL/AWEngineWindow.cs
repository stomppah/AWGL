﻿using Assimp;
using Assimp.Configs;
using AWGL.Managers;
using AWGL.Utilities;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace AWGL
{
    /// <summary>
    /// Inherit from here to get started.
    /// This is the main interface to the system.
    /// </summary>
    public abstract class AWEngineWindow : GameWindow, IDisposable
    {

        public static string AppName { get { return "AWEngine"; } }

        public int ScreenWidth { get { return this.ClientSize.Width; } }
        public int ScreenHeight { get { return this.ClientSize.Height; } }
        
        int modelviewMatrixLocation, projectionMatrixLocation;

        protected Matrix4 projectionMatrix, modelviewMatrix;
        protected ShaderManager shaderManager;
        protected PreciseTimer m_Timer;
        protected AWGL.Utilities.Camera camera;
        protected List<Key> keyList;

        private Vector3 m_sceneCenter, m_sceneMin, m_sceneMax;
        private Scene m_model;
        private float m_angle;
        private int m_displayList;
        private int m_texId;
        
        public AWEngineWindow(int height, int width, int major, int minor)
            : base(height, width, new GraphicsMode(32, 24, 8, 8), AWEngineWindow.AppName, GameWindowFlags.Default, 
            DisplayDevice.Default, 0, 0, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        { }

        #region Load everything here
        protected override void OnLoad(System.EventArgs e)
        {
            m_Timer = new PreciseTimer();

            //CameraManager
            camera = new AWGL.Utilities.Camera();
            
            // InputManager
            keyList = new List<Key>();
            Keyboard.KeyDown += HandleKeyDown;
            Keyboard.KeyUp += HandleKeyUp;

            //CreateShaders();
                                                                                                            //"Content/Models/Vehicles/Jupiter2/jupe2/Jupiter 2 Plus.obj"
            String fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Content/Models/Characters/bunny/reconstruction/bun_zipper.ply");

            AssimpContext importer = new AssimpContext();
            importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));
            m_model = importer.ImportFile(fileName, PostProcessPreset.TargetRealTimeMaximumQuality);
            ComputeBoundingBox();

            //// Other state
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(.1f, 0f, .1f, 0f);
           

#if Debug
            AWLogger.WriteLine("...Exiting OnLoad"); 
#endif      
            Initialise();
        }

        public abstract void Initialise();
        

        private void CreateShaders()
        {
            shaderManager = new ShaderManager("opentk-vs", "opentk-fs");

            GL.UseProgram(shaderManager.ProgramHandle);
            QueryMatrixLocations();

            float aspectRatio = ScreenWidth / (float)(ScreenHeight);
            SetProjectionMatrix(Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, aspectRatio, 1, 64));
            SetModelviewMatrix(Matrix4.CreateTranslation(0, 0, 5));
        }

        protected void QueryMatrixLocations()
        {
            projectionMatrixLocation = GL.GetUniformLocation(shaderManager.ProgramHandle, "projection_matrix");
            modelviewMatrixLocation = GL.GetUniformLocation(shaderManager.ProgramHandle, "modelview_matrix");
        }

        public void SetModelviewMatrix(Matrix4 matrix)
        {
            modelviewMatrix = matrix;
            GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelviewMatrix);
        }

        protected void SetProjectionMatrix(Matrix4 matrix)
        {
            projectionMatrix = matrix;
            GL.UniformMatrix4(projectionMatrixLocation, false, ref projectionMatrix);
        }

        #endregion

        #region Game Loop
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            #region input
            if (Focused)
            {
                Point center = new Point(Bounds.Left + Bounds.Width / 2, Bounds.Top + Bounds.Height / 2);
                Point delta = new Point(center.X - Cursor.Position.X, center.Y - Cursor.Position.Y);

                camera.AddRotation(delta.X, delta.Y);
                ResetCursor();
            }

            MoveCamera();

            //setmodelviewmatrix(matrix4.createrotationy((float)e.time) * modelviewmatrix);
            #endregion

            m_angle += 25f * (float)e.Time;
            if (m_angle > 360)
            {
                m_angle = 0.0f;
            }

            UpdateFrame(m_Timer.GetElapsedTime());
        }

        new public abstract void UpdateFrame(float elapsedTime);

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            
            GL.Viewport(0, 0, ScreenWidth, ScreenHeight);

            Title = AWEngineWindow.AppName +

                " OpenGL: " + GL.GetString(StringName.Version) +
                " GLSL: " + GL.GetString(StringName.ShadingLanguageVersion) +
                " FPS: " + string.Format("{0:F}", 1.0 / e.Time);
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // Single call to StateRenderer to take place here.
            
            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Normalize);
            GL.FrontFace(FrontFaceDirection.Ccw);

            GL.MatrixMode(MatrixMode.Modelview);
            Matrix4 lookat = camera.GetViewMatrix();
            GL.LoadMatrix(ref lookat);

            //GL.Rotate(m_angle, 0.0f, 1.0f, 0.0f);

            float tmp = m_sceneMax.X - m_sceneMin.X;
            tmp = Math.Max(m_sceneMax.Y - m_sceneMin.Y, tmp);
            tmp = Math.Max(m_sceneMax.Z - m_sceneMin.Z, tmp);
            tmp = 1.0f / tmp;
            GL.Scale(tmp * 20, tmp * 20, tmp * 20);

            GL.Translate(-m_sceneCenter);

            if (m_displayList == 0)
            {
                m_displayList = GL.GenLists(1);
                GL.NewList(m_displayList, ListMode.Compile);
                RecursiveRender(m_model, m_model.RootNode);
                GL.EndList();
            }

            GL.CallList(m_displayList);

            RenderFrame(m_Timer.GetElapsedTime());

            SwapBuffers();
        }

        new public abstract void RenderFrame(float elapsedTime);

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, ScreenWidth, ScreenHeight);

            float widthToHeight = ScreenWidth / (float)ScreenHeight;
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, widthToHeight, 1, 64);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perspective);
        }

        #endregion

        #region GameWindow.Dispose
        public override void Dispose()
        {
            base.Dispose();
            //shaderManager.Dispose();
        } 
        #endregion

        #region Input Control
        private void HandleKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            keyList.Add(e.Key);
        }

        private void HandleKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            for (int count = 0; count < keyList.Count; count++)
            {
                if (keyList[count] == e.Key)
                {
                    keyList.Remove(keyList[count]);
                }
            }
        }

        private void MoveCamera()
        {
            if (keyList.Count > 0) 
            {
                foreach (Key key in keyList)
                {

                    switch (key)
                    {
                        case Key.Escape:
                            Exit();
                            break;

                        case Key.W:
                            camera.Move(0f, 0.1f, 0f);
                            break;

                        case Key.A:
                            camera.Move(-0.1f, 0f, 0f);
                            break;

                        case Key.S:
                            camera.Move(0f, -0.1f, 0f);
                            break;

                        case Key.D:
                            camera.Move(0.1f, 0f, 0f);
                            break;

                        case Key.Q:
                            camera.Move(0f, 0f, 0.1f);
                            break;

                        case Key.E:
                            camera.Move(0f, 0f, -0.1f);
                            break;

                        case Key.F11:
                            if (this.WindowState == OpenTK.WindowState.Normal)
                                this.WindowState = OpenTK.WindowState.Fullscreen;
                            else
                                this.WindowState = OpenTK.WindowState.Normal;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void ResetCursor()
        {
            System.Windows.Forms.Cursor.Position = new Point(Bounds.Left + Bounds.Width / 2, Bounds.Top + Bounds.Height / 2);
        }

        protected override void OnFocusedChanged(EventArgs e)
        {
            base.OnFocusedChanged(e);

            if (Focused)
            {
                ResetCursor();
            }
        } 
        #endregion

        private void ComputeBoundingBox()
        {
            m_sceneMin = new Vector3(1e10f, 1e10f, 1e10f);
            m_sceneMax = new Vector3(-1e10f, -1e10f, -1e10f);
            Matrix4 identity = Matrix4.Identity;

            ComputeBoundingBox(m_model.RootNode, ref m_sceneMin, ref m_sceneMax, ref identity);

            m_sceneCenter.X = (m_sceneMin.X + m_sceneMax.X) / 2.0f;
            m_sceneCenter.Y = (m_sceneMin.Y + m_sceneMax.Y) / 2.0f;
            m_sceneCenter.Z = (m_sceneMin.Z + m_sceneMax.Z) / 2.0f;
        }

        private void ComputeBoundingBox(Node node, ref Vector3 min, ref Vector3 max, ref Matrix4 trafo)
        {
            Matrix4 prev = trafo;
            trafo = Matrix4.Mult(prev, FromMatrix(node.Transform));

            if (node.HasMeshes)
            {
                foreach (int index in node.MeshIndices)
                {
                    Mesh mesh = m_model.Meshes[index];
                    for (int i = 0; i < mesh.VertexCount; i++)
                    {
                        Vector3 tmp = FromVector(mesh.Vertices[i]);
                        Vector3.Transform(ref tmp, ref trafo, out tmp);

                        min.X = Math.Min(min.X, tmp.X);
                        min.Y = Math.Min(min.Y, tmp.Y);
                        min.Z = Math.Min(min.Z, tmp.Z);

                        max.X = Math.Max(max.X, tmp.X);
                        max.Y = Math.Max(max.Y, tmp.Y);
                        max.Z = Math.Max(max.Z, tmp.Z);
                    }
                }
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                ComputeBoundingBox(node.Children[i], ref min, ref max, ref trafo);
            }
            trafo = prev;
        }

        private void RecursiveRender(Scene scene, Node node)
        {
            Matrix4 m = FromMatrix(node.Transform);
            m.Transpose();
            GL.PushMatrix();
            GL.MultMatrix(ref m);

            if (node.HasMeshes)
            {
                foreach (int index in node.MeshIndices)
                {
                    Mesh mesh = scene.Meshes[index];
                    ApplyMaterial(scene.Materials[mesh.MaterialIndex]);

                    if (mesh.HasNormals)
                    {
                        GL.Enable(EnableCap.Lighting);
                    }
                    else
                    {
                        GL.Disable(EnableCap.Lighting);
                    }

                    bool hasColors = mesh.HasVertexColors(0);
                    if (hasColors)
                    {
                        GL.Enable(EnableCap.ColorMaterial);
                    }
                    else
                    {
                        GL.Disable(EnableCap.ColorMaterial);
                    }

                    bool hasTexCoords = mesh.HasTextureCoords(0);

                    foreach (Face face in mesh.Faces)
                    {
                        BeginMode faceMode;
                        switch (face.IndexCount)
                        {
                            case 1:
                                faceMode = BeginMode.Points;
                                break;
                            case 2:
                                faceMode = BeginMode.Lines;
                                break;
                            case 3:
                                faceMode = BeginMode.Triangles;
                                break;
                            default:
                                faceMode = BeginMode.Polygon;
                                break;
                        }

                        GL.Begin(faceMode);
                        for (int i = 0; i < face.IndexCount; i++)
                        {
                            int indice = face.Indices[i];
                            if (hasColors)
                            {
                                Color4 vertColor = FromColor(mesh.VertexColorChannels[0][indice]);
                            }
                            if (mesh.HasNormals)
                            {
                                Vector3 normal = FromVector(mesh.Normals[indice]);
                                GL.Normal3(normal);
                            }
                            if (hasTexCoords)
                            {
                                Vector3 uvw = FromVector(mesh.TextureCoordinateChannels[0][indice]);
                                GL.TexCoord2(uvw.X, 1 - uvw.Y);
                            }
                            Vector3 pos = FromVector(mesh.Vertices[indice]);
                            GL.Vertex3(pos);
                        }
                        GL.End();
                    }
                }
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                RecursiveRender(m_model, node.Children[i]);
            }
        }

        private void LoadTexture(String fileName)
        {
            fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName);
            if (!File.Exists(fileName))
            {
                return;
            }
            Bitmap textureBitmap = new Bitmap(fileName);
            BitmapData TextureData =
                            textureBitmap.LockBits(
                            new System.Drawing.Rectangle(0, 0, textureBitmap.Width, textureBitmap.Height),
                            System.Drawing.Imaging.ImageLockMode.ReadOnly,
                            System.Drawing.Imaging.PixelFormat.Format24bppRgb
                    );
            m_texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, m_texId);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, textureBitmap.Width, textureBitmap.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, TextureData.Scan0);
            textureBitmap.UnlockBits(TextureData);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        private void ApplyMaterial(Material mat)
        {
            if (mat.GetMaterialTextureCount(TextureType.Diffuse) > 0)
            {
                TextureSlot tex;
                if (mat.GetMaterialTexture(TextureType.Diffuse, 0, out tex))
                    LoadTexture(tex.FilePath);
            }

            Color4 color = new Color4(.8f, .8f, .8f, 1.0f);
            if (mat.HasColorDiffuse)
            {
                color = FromColor(mat.ColorDiffuse);
            }
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, color);

            color = new Color4(0, 0, 0, 1.0f);
            if (mat.HasColorSpecular)
            {
                color = FromColor(mat.ColorSpecular);
            }
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, color);

            color = new Color4(.2f, .2f, .2f, 1.0f);
            if (mat.HasColorAmbient)
            {
                color = FromColor(mat.ColorAmbient);
            }
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, color);

            color = new Color4(0, 0, 0, 1.0f);
            if (mat.HasColorEmissive)
            {
                color = FromColor(mat.ColorEmissive);
            }
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, color);

            float shininess = 1;
            float strength = 1;
            if (mat.HasShininess)
            {
                shininess = mat.Shininess;
            }
            if (mat.HasShininessStrength)
            {
                strength = mat.ShininessStrength;
            }

            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, shininess * strength);
        }

        private Matrix4 FromMatrix(Matrix4x4 mat)
        {
            Matrix4 m = new Matrix4();
            m.M11 = mat.A1;
            m.M12 = mat.A2;
            m.M13 = mat.A3;
            m.M14 = mat.A4;
            m.M21 = mat.B1;
            m.M22 = mat.B2;
            m.M23 = mat.B3;
            m.M24 = mat.B4;
            m.M31 = mat.C1;
            m.M32 = mat.C2;
            m.M33 = mat.C3;
            m.M34 = mat.C4;
            m.M41 = mat.D1;
            m.M42 = mat.D2;
            m.M43 = mat.D3;
            m.M44 = mat.D4;
            return m;
        }

        private Vector3 FromVector(Vector3D vec)
        {
            Vector3 v;
            v.X = vec.X;
            v.Y = vec.Y;
            v.Z = vec.Z;
            return v;
        }

        private Color4 FromColor(Color4D color)
        {
            Color4 c;
            c.R = color.R;
            c.G = color.G;
            c.B = color.B;
            c.A = color.A;
            return c;
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            GL.DeleteTexture(m_texId);
        }
    }
}