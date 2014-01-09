﻿using ObjLoader.Loader.Loaders;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace AWGL
{
    public static class AWGL 
    {
        
        public static void Main(string[] args)
        {
            using (Display display = new Display())
            {
                display.Run(30.0);
            }
        }
        
    }
}
