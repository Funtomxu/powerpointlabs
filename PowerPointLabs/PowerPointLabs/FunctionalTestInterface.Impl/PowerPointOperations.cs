﻿using System;
using System.Collections.Generic;
using FunctionalTestInterface;
using Microsoft.Office.Interop.PowerPoint;
using PowerPointLabs.Models;
using System.IO;
using System.Windows.Forms;

namespace PowerPointLabs.FunctionalTestInterface.Impl
{
    [Serializable]
    class PowerPointOperations : MarshalByRefObject, IPowerPointOperations
    {
        public void EnterFunctionalTest()
        {
            PowerPointCurrentPresentationInfo.IsInFunctionalTest = true;
        }

        public void ExitFunctionalTest()
        {
            PowerPointCurrentPresentationInfo.IsInFunctionalTest = false;
        }

        public bool IsInFunctionalTest()
        {
            return PowerPointCurrentPresentationInfo.IsInFunctionalTest;
        }

        public void ClosePresentation()
        {
            EnterFunctionalTest();
            Globals.ThisAddIn.Application.ActivePresentation.Close();
        }

        public void ActivatePresentation()
        {
            MessageBox.Show(new Form() { TopMost = true },
                "###__DO_NOT_OPEN_OTHER_WINDOW__###\n" +
                "###___DURING_FUNCTIONAL_TEST___###", "PowerPointLabs FT");
        }

        public Slide GetCurrentSlide()
        {
            return PowerPointCurrentPresentationInfo.CurrentSlide.GetNativeSlide();
        }

        public Slide SelectSlide(int index)
        {
            var slides = PowerPointPresentation.Current.Slides;
            for (int i = 0; i <= slides.Count; i++)
            {
                if (i == (index - 1))
                {
                    var slide = slides[i].GetNativeSlide();
                    slide.Select();
                    return slide;
                }
            }
            return null;
        }

        public Slide SelectSlide(string slideName)
        {
            var slides = PowerPointPresentation.Current.Slides;
            for (int i = 0; i <= slides.Count; i++)
            {
                if (slideName == slides[i].Name)
                {
                    var slide = slides[i].GetNativeSlide();
                    slide.Select();
                    return slide;
                }
            }
            return null;
        }

        public Selection GetCurrentSelection()
        {
            return PowerPointCurrentPresentationInfo.CurrentSelection;
        }

        public ShapeRange SelectShapes(string shapeName)
        {
            var nameList = new List<String>();
            var shapes = PowerPointCurrentPresentationInfo.CurrentSlide.Shapes;
            foreach (Shape sh in shapes)
            {
                if (sh.Name == shapeName)
                {
                    nameList.Add(sh.Name);
                }
            }
            return SelectShapes(nameList);
        }

        public ShapeRange SelectShapes(List<string> shapeNames)
        {
            var range = PowerPointCurrentPresentationInfo
                .CurrentSlide.Shapes.Range(shapeNames.ToArray());

            if (range.Count > 0)
            {
                range.Select();
                return range;
            }
            return null;
        }

        public ShapeRange SelectShapesByPrefix(string prefix)
        {
            var nameList = new List<String>();
            var shapes = PowerPointCurrentPresentationInfo.CurrentSlide.Shapes;
            foreach (Shape sh in shapes)
            {
                if (sh.Name.StartsWith(prefix))
                {
                    nameList.Add(sh.Name);
                }
            }
            return SelectShapes(nameList);
        }

        public FileInfo ExportSelectedShapes()
        {
            var shapes = PowerPointCurrentPresentationInfo.CurrentSelection.ShapeRange;
            var hashCode = DateTime.Now.GetHashCode();
            var pathName = Path.GetTempPath() + "shapeName" + hashCode;
            shapes.Export(pathName, PpShapeFormat.ppShapeFormatPNG);
            return new FileInfo(pathName);
        }
    }
}