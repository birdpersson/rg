// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using SharpGL;
using SharpGL.SceneGraph.Quadrics;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        private enum TextureObjects { Grass = 0, Plastic };
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;

        /// <summary>
        ///	 Identifikatori OpenGL tekstura
        /// </summary>
        private uint[] m_textures = null;

        /// <summary>
        ///	 Putanje do slika koje se koriste za teksture
        /// </summary>
        private string[] m_textureFiles = { "..//..//images//grass.jpg", "..//..//images//plastic.jpg" };

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 70.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
            m_textures = new uint[m_textureCount];
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            //gl.LookAt(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f);
            //gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);

            SetupLighting(gl);
            SetupTextures(gl);

            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            gl.ShadeModel(OpenGL.GL_SMOOTH);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            m_scene.LoadScene();
            m_scene.Initialize();
        }

        private void SetupLighting(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_LIGHTING);

            float[] light0pos = new float[] { 40.0f, 10.0f, 10.0f, 1.0f };
            float[] light0ambient = new float[] { 0.4f, 0.4f, 0.4f, 1.0f };
            float[] light0diffuse = new float[] { 0.3f, 0.3f, 0.3f, 1.0f };
            float[] light0specular = new float[] { 0.8f, 0.8f, 0.8f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);

            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Enable(OpenGL.GL_NORMALIZE);
        }

        private void SetupTextures(OpenGL gl)
        {
            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);

                // Ucitaj sliku i podesi parametre teksture
                Bitmap image = new Bitmap(m_textureFiles[i]);
                // rotiramo sliku zbog koordinantog sistema opengl-a
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // RGBA format (dozvoljena providnost slike tj. alfa kanal)
                BitmapData imageData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);		// Linear Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);      // Linear Filtering
                
                image.UnlockBits(imageData);
                image.Dispose();
            }

            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.LookAt(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f);
            //gl.Enable(OpenGL.GL_AUTO_NORMAL);

            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            DrawGround(gl);
            DrawGoal(gl);
            //DrawText(gl);

            m_scene.Draw();
            gl.PopMatrix();
            // Oznaci kraj iscrtavanja
            gl.Flush();
        }

        private void DrawGround(OpenGL gl)
        {
            gl.Color(0.0f, 0.0f, 0.0f);
            gl.PushMatrix();
            gl.Translate(0, -1f, 0);

            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.PushMatrix();
            gl.Scale(20.0f, 20.0f, 20.0f);

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Grass]);
            gl.Begin(OpenGL.GL_QUADS);

            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(35f, 0, 35f);

            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(35f, 0, -35f);

            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(-35f, 0, -35f);

            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-35f, 0, 35f);

            gl.End();
            gl.PopMatrix();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.PopMatrix();
        }

        private void DrawGoal(OpenGL gl)
        {
            gl.Color(1.0f, 1.0f, 1.0f);

            gl.PushMatrix();
            //gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Plastic]);

            gl.Translate(-12, -1, -30);
            gl.Rotate(-90, 0, 0);

            Cylinder left = new Cylinder();
            left.Height = 20f;
            left.BaseRadius = 0.4f;
            left.TopRadius = 0.4f;
            left.CreateInContext(gl);
            left.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

            gl.Translate(0, 0, 20);
            Disk lcap = new Disk();
            lcap.OuterRadius = 0.4f;
            lcap.CreateInContext(gl);
            lcap.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

            //TODO: move tlcap here

            gl.PopMatrix();
            gl.PushMatrix();

            gl.Translate(12, -1, -30);
            gl.Rotate(-90, 0, 0);

            Cylinder right = new Cylinder();
            right.Height = 20f;
            right.BaseRadius = 0.4f;
            right.TopRadius = 0.4f;
            right.CreateInContext(gl);
            right.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

            //gl.PopMatrix();
            //gl.PushMatrix();

            gl.Translate(0, 0, 20);
            Disk rcap = new Disk();
            rcap.OuterRadius = 0.4f;
            rcap.CreateInContext(gl);
            rcap.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

            //disk cap top right
            gl.PushMatrix();
            gl.Rotate(0, 90, 0);
            Disk trcap = new Disk();
            trcap.OuterRadius = 0.4f;
            trcap.CreateInContext(gl);
            trcap.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            gl.Rotate(0, -90, 0);

            Cylinder top = new Cylinder();
            top.Height = 24f;
            top.BaseRadius = 0.4f;
            top.TopRadius = 0.4f;
            top.CreateInContext(gl);
            top.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

            //disk cap top left
            gl.PushMatrix();
            gl.Translate(0, 0, 24);
            Disk tlcap = new Disk();
            tlcap.OuterRadius = 0.4f;
            tlcap.CreateInContext(gl);
            tlcap.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            gl.PopMatrix();
        }

        private void DrawText(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Viewport(m_width / 2, m_height / 2, m_width / 2, m_height / 2);
            // https://github.com/dwmkerr/sharpgl/issues/168
            gl.DrawText(10, 70, 0.0f, 0.0f, 0.0f, "Tahoma", 10, "Predmet: Racunarska grafika");
            gl.DrawText(10, 60, 0.0f, 0.0f, 0.0f, "Tahoma", 10, "Sk.god: 2020/21");
            gl.DrawText(10, 50, 0.0f, 0.0f, 0.0f, "Tahoma", 10, "Ime: Aleksandar");
            gl.DrawText(10, 40, 0.0f, 0.0f, 0.0f, "Tahoma", 10, "Prezime: Vujasin");
            gl.DrawText(10, 30, 0.0f, 0.0f, 0.0f, "Tahoma", 10, "Sifra zad: 17.1");
            gl.PopMatrix();
        }

        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(45f, (double)width / height, 0.5f, 20000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
