/*
    Matali Physics Demo
    Copyright (c) 2013 KOMIRES Sp. z o. o.
 */
using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Komires.MataliPhysics;

namespace MataliPhysicsDemo
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ForceField1
    {
        Demo demo;
        PhysicsScene scene;

        public ForceField1(Demo demo)
        {
            this.demo = demo;
        }

        public void Initialize(PhysicsScene scene)
        {
            this.scene = scene;
        }

        public static void CreateShapes(Demo demo, PhysicsScene scene)
        {
        }

        public void Create()
        {
            Shape sphere = scene.Factory.ShapeManager.Find("Sphere");
            Shape cylinderY = scene.Factory.ShapeManager.Find("CylinderY");
            Shape userShape1 = scene.Factory.ShapeManager.Find("UserShape 1");
            Shape userShape2 = scene.Factory.ShapeManager.Find("UserShape 2");

            PhysicsObject objectRoot = null;
            PhysicsObject objectBase = null;

            objectRoot = scene.Factory.PhysicsObjectManager.Create("ForceField 1");

            objectBase = scene.Factory.PhysicsObjectManager.Create("ForceField 1 Shape");
            objectRoot.AddChildPhysicsObject(objectBase);
            objectBase.Shape = userShape1;
            objectBase.UserDataStr = "UserShape1";
            objectBase.Material.RigidGroup = true;
            objectBase.InitLocalTransform.SetPosition(-20.0f, 20.0f, 70.0f);
            objectBase.InitLocalTransform.SetScale(2.0f);
            objectBase.Integral.SetDensity(100.0f);
            objectBase.EnableBreakRigidGroup = false;
            objectBase.CreateSound(true);

            objectBase = scene.Factory.PhysicsObjectManager.Create("ForceField 1 Field");
            objectRoot.AddChildPhysicsObject(objectBase);
            objectBase.Shape = sphere;
            objectBase.UserDataStr = "Sphere";
            objectBase.Material.UserDataStr = "Blue";
            objectBase.Material.RigidGroup = true;
            objectBase.Material.TransparencyFactor = 0.5f;
            objectBase.InitLocalTransform.SetPosition(-20.0f, 20.0f, 70.0f);
            objectBase.InitLocalTransform.SetScale(10.0f, 10.0f, 10.0f);
            objectBase.EnableBreakRigidGroup = false;
            objectBase.EnableCollisionResponse = false;
            objectBase.EnableCursorInteraction = false;

            scene.UpdateFromInitLocalTransform(objectRoot);
        }
    }
}
