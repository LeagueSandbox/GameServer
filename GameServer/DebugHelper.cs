using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Numerics;
using System.Drawing;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.RAF;
using static InibinSharp.AIMesh;


namespace LeagueSandbox
{
    /// <summary>
    /// Debug helper class. Provides range of functions to help debugging
    /// </summary>
    unsafe class DebugHelper
    {

        private DebugHelper()
        {
           
        }

        
        public void ImageAddTriangles(AIMesh mesh,List<int> listOfTris,Color color,List<Vector2> points,List<Vector2> pointPath)
        {

            Bitmap bitmap = createMapBitmap(mesh);
            float pxPerUnit = mesh.getWidth() / bitmap.Width;

            Graphics g = Graphics.FromImage(bitmap);
            int height = bitmap.Height;
            

            foreach (int i in listOfTris)
            {
                Triangle tri = mesh.getTriangle(i);

                Pen p = new Pen(color, 3);

                Point[] triPoints = {new Point((int)(tri.Face.v1.X/pxPerUnit),height -1 -(int)(tri.Face.v1.Z/pxPerUnit)),
            new Point((int)(tri.Face.v2.X/pxPerUnit),height -1 -(int)(tri.Face.v2.Z/pxPerUnit)),
            new Point((int)(tri.Face.v3.X/pxPerUnit),height -1 -(int)(tri.Face.v3.Z/pxPerUnit))};


                g.DrawPolygon(p, triPoints);
            }

            foreach(Vector2 point in points)
            {
                Point[] fromPoints = {new Point((int)(point.X/pxPerUnit-5),height -1 -(int)(point.Y/pxPerUnit-5)),
            new Point((int)(point.X/pxPerUnit-5),height -1 -(int)(point.Y/pxPerUnit+5)),
            new Point((int)(point.X/pxPerUnit+5),height -1 -(int)(point.Y/pxPerUnit+5)),
            new Point((int)(point.X/pxPerUnit+5),height -1 -(int)(point.Y/pxPerUnit-5)),};

                //g.DrawPolygon(new Pen(Color.Red), fromPoints);
                g.FillPolygon(new SolidBrush(Color.Red), fromPoints);
            }

            Vector2 lastPoint = new Vector2();
            if (pointPath.Count > 0)
                lastPoint = pointPath[0];

            for(int i=1;i<pointPath.Count;i++)
            {
                Vector2 currentPoint = pointPath[i];
                g.DrawLine(new Pen(Color.Blue, 2), new Point((int)(lastPoint.X / pxPerUnit), height - 1 - (int)(lastPoint.Y / pxPerUnit)),
                    new Point((int)(currentPoint.X / pxPerUnit), height - 1 - (int)(currentPoint.Y / pxPerUnit)));
                lastPoint = currentPoint;
            }


            g.Flush();

            try
            {
                bitmap.Save("C:/mapImage.png");
                bitmap.Dispose();
            }
            catch (System.Exception ex)
            {
                Logger.LogCoreError("Could not save debug bitmap" + ex);
            }
        }


        private Bitmap createMapBitmap(AIMesh mesh)
        {
            float width = mesh.getWidth();
            float height = mesh.getHeight();
            int desiredSize = 2048;
            
            float pxPerUnit = width / desiredSize;

            Bitmap bitmap = new Bitmap((int)(width / pxPerUnit), (int)(height / pxPerUnit));
            Graphics g = Graphics.FromImage(bitmap);
            g.ScaleTransform(1, -1); //flip Y
            g.TranslateTransform(0, -(float)bitmap.Height);
            g.Clear(Color.Black);

            int h = bitmap.Height;

            g.SmoothingMode = SmoothingMode.None;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            //display all navmesh triangles
            for (int i = 0; i < mesh.TriangleCount; i++)
            {
                Triangle tri = mesh.getTriangle(i);

                Pen p = new Pen(Color.White, 2);

                Point[] triPoints = {new Point((int)(tri.Face.v1.X/pxPerUnit), (int)(tri.Face.v1.Z/pxPerUnit)),
            new Point((int)(tri.Face.v2.X/pxPerUnit), (int)(tri.Face.v2.Z/pxPerUnit)),
            new Point((int)(tri.Face.v3.X/pxPerUnit), (int)(tri.Face.v3.Z/pxPerUnit))};


                g.DrawPolygon(p, triPoints);

                Vector2 triCenter = mesh.GetTriCenter(i);

                g.ScaleTransform(1, -1); //flip Y
                g.TranslateTransform(0, -(float)bitmap.Height);

                g.DrawString(i.ToString(), new Font(FontFamily.GenericSansSerif, 10), new SolidBrush(Color.White), new Point((int)(triCenter.X / pxPerUnit), h - 1 - (int)(triCenter.Y / pxPerUnit)));

                g.ScaleTransform(1, -1); //flip Y
                g.TranslateTransform(0, -(float)bitmap.Height);
            }

            g.Flush();
            return bitmap;
        }
       

     
        public static DebugHelper getInstance()
        {
            if (_instance == null)
                _instance = new DebugHelper();

            return _instance;
        }

            private static DebugHelper _instance;

        }

}
