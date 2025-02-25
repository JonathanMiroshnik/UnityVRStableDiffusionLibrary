using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;


/// <summary>
/// A collection of Classes and functions that are useful throughout the project and are based on default Unity types of Objects.
/// </summary>
public static class GeneralGameLibraries : System.Object
{
    /// <summary>
    /// Gets a random sublist of size elementsCount from given list
    /// </summary>
    /// <returns>Sublist of size elementsCount from given list</returns>
    public static List<T> GetRandomElements<T>(List<T> list, int elementsCount)
    {
        if (elementsCount <= 0) return null;
        if (list == null) return null;
        if (list.Count() < elementsCount)
        {
            elementsCount = list.Count();
        }
        return list.OrderBy(arg => Guid.NewGuid()).Take(elementsCount).ToList();
    }

    /// <summary>
    /// Opens an folder and retreives all the Audio Clips within for easy use through its Dictionary
    /// </summary>
    public class AudioClipsLibrary : UnityEngine.Object
    {
        public string AudioClipFolder;
        public Dictionary<string, AudioClip> AudioClips;

        public AudioClipsLibrary(string curAudioClipFolder = "Sounds/Voiceover")
        {
            if (curAudioClipFolder == "")
            {
                Debug.LogError("Choose a proper Audio Clip Folder");
                return;
            }
            AudioClipFolder = curAudioClipFolder;
            AudioClips = new Dictionary<string, AudioClip>();

            GetAudioClips(AudioClipFolder);
        }

        /// <summary>
        /// Gets all the Audio Clips from the given folder and adds it to the AudioClips Dictionary
        /// </summary>
        /// <param name="audioClipFolder">Folder to get Audio Clips from</param>
        private void GetAudioClips(string audioClipFolder)
        {   
            var audioClipFileNames = Resources.LoadAll(audioClipFolder, typeof(AudioClip));

            foreach (AudioClip audioClip in audioClipFileNames)
            {
                AudioClips[audioClip.name] = Resources.Load<AudioClip>(audioClipFolder + "/" + audioClip.name);
            }
        }
    }

    /// <summary>
    /// Provides functions for general Texture manipulations
    /// </summary>
    public class TextureManipulationLibrary : UnityEngine.Object
    {
        // https://stackoverflow.com/questions/44264468/convert-rendertexture-to-texture2d

        /// <summary>
        /// Converts a given RenderTexture to a Texture2D
        /// </summary>
        /// <param name="rTex">RenderTexture to convert</param>
        /// <returns>Converted rTex to Texture2D</returns>
        public static Texture2D toTexture2D(RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
            var old_rt = RenderTexture.active;
            RenderTexture.active = rTex;

            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();

            RenderTexture.active = old_rt;
            tex.name = rTex.name;
            return tex;
        }

        /// <summary>
        /// Converts a given Texture to a Texture2D
        /// </summary>
        /// <param name="inTex">Texture to convert</param>
        /// <returns>Converted inTex to Texture2D</returns>
        public static Texture2D toTexture2D(Texture inTex)
        {
            RenderTexture rTex = new RenderTexture(inTex.width, inTex.height, 4);
            Graphics.Blit(inTex, rTex);

            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
            // ReadPixels looks at the active RenderTexture.
            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();

            tex.name = inTex.name;

            return tex;
        }

        public static Texture2D DeCompress(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);

            readableText.name = source.name;

            return readableText;
        }

        /// <summary>
        /// Copies a Texture2D a returns a new instance of the same Texture2D
        /// </summary>
        public static Texture2D CopyTexture(Texture2D texture)
        {
            if (texture == null) return null;

            Texture2D copyTexture = new Texture2D(texture.width, texture.height);
            copyTexture.SetPixels(texture.GetPixels());
            copyTexture.Apply();
            copyTexture.name = texture.name;

            return copyTexture;
        }

        // TODO: allow height = 768?
        // Default image size for Diffusion
        public const int DEFAULT_HEIGHT = 512;
        public const int DEFAULT_WIDTH = 512;

        /// <summary>
        /// Resizes a Texture2D to a default size
        /// </summary>
        /// <param name="texture">Input Texture2D</param>
        /// <returns>New instance of Texture2D with default size</returns>
        public static Texture2D Resize(Texture2D texture)
        {
            return Resize(texture, DEFAULT_WIDTH, DEFAULT_HEIGHT);
        }

        // TODO: delete this function?
        /// <summary>
        /// Resizes a Texture2D to a given size
        /// </summary>
        /// <param name="texture">Input Texture2D</param>
        /// <param name="newWidth">Given new width</param>
        /// <param name="newHeight">Given new height</param>
        /// <returns>New instance of Texture2D with given size</returns>
        /*public static Texture2D Resize(Texture2D texture, int newWidth, int newHeight)
        {            
            if (texture == null) return null;
            if (texture.height == newHeight && texture.width == newWidth) return texture;
            Texture2D newTexture = CopyTexture(texture);
            newTexture.Reinitialize(newWidth, newHeight);
            return newTexture;
        }*/


        //--------------------------------

        /// <summary>
        /// Resizes a Texture2D to a given size
        /// </summary>
        /// <param name="texture2D">Input Texture2D</param>
        /// <param name="targetX">Given new width</param>
        /// <param name="targetY">Given new height</param>
        /// <returns>New instance of Texture2D with given size</returns>
        public static Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
        {
            Texture2D newTexture = CopyTexture(texture2D);

            RenderTexture rt = new RenderTexture(targetX, targetY, 24);
            RenderTexture.active = rt;
            Graphics.Blit(newTexture, rt);
            Texture2D result = new Texture2D(targetX, targetY);
            result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
            result.Apply();

            result.name = newTexture.name;

            return result;
        }

        //--------------------------------

        // configure with raw, jpg, png, or ppm (simple raw format)
        enum Format { RAW, JPG, PNG, PPM };

        /// <summary>
        /// Captures an image with the Camera it is attached to, saves it to folder and returns it.
        /// </summary>
        public static Texture2D CaptureScreenshot(Camera camera)
        {
            if (GameManager.getInstance() == null) return null;

            // 4k = 3840 x 2160   1080p = 1920 x 1080
            int captureWidth = 512;
            int captureHeight = 512;

            Format format = Format.PNG;

            // folder to write output (defaults to data path)
            string folder = "Assets/ComfyUILib/Screenshots";

            // private vars for screenshot
            Rect rect;
            RenderTexture renderTexture = null;
            Texture2D screenShot;

            // creates off-screen render texture that can rendered into
            rect = new Rect(0, 0, captureWidth, captureHeight);
            renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
            screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);

            // get main camera and manually render scene into rt
            camera.targetTexture = renderTexture;
            camera.Render();

            // read pixels will read from the currently active render texture so make our offscreen 
            // render texture active and then read the pixels
            RenderTexture.active = renderTexture;
            screenShot.ReadPixels(rect, 0, 0);

            // reset active camera texture and render texture
            camera.targetTexture = null;
            RenderTexture.active = null;

            Debug.Log("screen 1");

            // get our unique filename
            string filename = folder + '/' + GameManager.getInstance().comfyOrganizer.UniqueImageName() + '.' + format.ToString().ToLower();

            // pull in our file header/data bytes for the specified image format (has to be done from main thread)
            byte[] fileHeader = null;
            byte[] fileData = null;
            if (format == Format.RAW)
            {
                fileData = screenShot.GetRawTextureData();
            }
            else if (format == Format.PNG)
            {
                fileData = screenShot.EncodeToPNG();
            }
            else if (format == Format.JPG)
            {
                fileData = screenShot.EncodeToJPG();
            }
            else // ppm
            {
                // create a file header for ppm formatted file
                string headerStr = string.Format("P6{0}{1}255", rect.width, rect.height);

                fileHeader = System.Text.Encoding.ASCII.GetBytes(headerStr);
                fileData = screenShot.GetRawTextureData();
            }

            // Create new thread to save the image to file (only operation that can be done in background)
            new System.Threading.Thread(() =>
            {
                // create file and write optional header with image bytes
                var f = System.IO.File.Create(filename);
                if (fileHeader != null) f.Write(fileHeader, 0, fileHeader.Length);
                f.Write(fileData, 0, fileData.Length);
                f.Close();
                Debug.Log(string.Format("Wrote screenshot {0} of size {1}", filename, fileData.Length));
            }).Start();

            int lastSlashIndex = filename.LastIndexOf('/');
            // Extract the file name by taking the substring after the last slash
            string cutFileName = filename.Substring(lastSlashIndex + 1);

            screenShot.name = cutFileName;

            return screenShot;
        }
    }

    public class GameObjectManipulationLibrary : MonoBehaviour
    {
        /// <summary>
        /// Helper function for ChangeOutline that destroys an existing outline on an outlined GameObject
        /// </summary>
        /// <param name="obj"></param>
        private static void DeleteOutline(GameObject obj)
        {
            if (obj == null) return;
            if (obj.TryGetComponent<Outline>(out Outline curOutline)) Destroy(curOutline);
        }

        /// <summary>
        /// Changes the outlining on a given GameObject
        /// </summary>
        /// <param name="obj">GameObject to change outlining on</param>
        /// <param name="gadgetSelection">Outlining to change onto</param>
        public static void ChangeOutline(GameObject obj, GadgetSelection gadgetSelection)
        {
            if (obj == null) return;

            // Only GameObjects with valid textures
            if (obj.GetComponent<Renderer>() == null) return;
            if (!obj.TryGetComponent<TextureTransition>(out TextureTransition curTransition))
            {
                if (obj.GetComponent<Renderer>().material.mainTexture == null && obj.GetComponent<DiffusableObject>() == null) return;
            }

            Color curColor = Color.black;
            float outlineWidth = 0;
            switch (gadgetSelection)
            {
                case GadgetSelection.unSelected:
                    DeleteOutline(obj);
                    return;
                case GadgetSelection.preSelected:
                    curColor = new Color(0, 0, 255); ;
                    outlineWidth = 20;
                    break;
                case GadgetSelection.selected:
                    curColor = new Color(0, 255, 0); ;
                    outlineWidth = 20;
                    break;
            }

            if (obj.TryGetComponent<Outline>(out Outline curOutline))
            {
                curOutline.OutlineColor = curColor;
                curOutline.OutlineWidth = outlineWidth;
                return;
            }

            Outline elseOutline = obj.AddComponent<Outline>();
            elseOutline.OutlineColor = curColor;
            elseOutline.OutlineWidth = outlineWidth;
        }
    }
}
