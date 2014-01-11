﻿using ObjLoader.Loader.Loaders;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

using AWGL.Scene;

namespace AWGL
{
    public static class AWGL 
    {
        [STAThread]
        public static void Main(string[] args)
        {
            using (Texture2DScene particles = new Texture2DScene())
            {
                particles.Run(30.0);
            }
        }

        
    }
}
