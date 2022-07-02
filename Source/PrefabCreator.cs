using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using MVR.FileManagementSecure;
using static VAM_Decal_Maker.PathHelper;
using System.Collections;
using UnityEngine;

namespace VAM_Decal_Maker
{
    public class PrefabCreator
    {
        Decal_Maker DM;
        private bool waitingOnScreenShot;

        public PrefabCreator(Decal_Maker DM)
        {
            this.DM = DM;
        }

        private JSONClass Recursive(JSONClass jc)
        {
            if (jc == null)
                return null;

            if(jc.HasKey("Path"))
                return jc;

            JSONNode[] a = jc.Childs.Where(x => x.GetType() == typeof(JSONArray)).ToArray();
            foreach (JSONArray a2 in a)
            {
                foreach (JSONNode v in a2)
                {
                    if(v.GetType() == typeof(JSONClass))
                    return Recursive((JSONClass) v);
                }
            }

            JSONNode[] c = jc.Childs.Where(x => x.GetType() == typeof(JSONClass)).ToArray();
            foreach (JSONClass c2 in c)
            { 
                return Recursive(c2);
            }
            return null;
        }


        public void Start(string prefabExample, string sourceImages)
        {
            string dirPath = FileManagerSecure.GetDirectoryName(prefabExample);

            string[] allFiles =  FileManagerSecure.GetFiles(sourceImages);
            
            List<string> imageFiles = allFiles.Where(x => x.EndsWith("png", StringComparison.OrdinalIgnoreCase) || x.EndsWith("jpg", StringComparison.OrdinalIgnoreCase)).ToList();

            List<string> prefabs = new List<string>();

            foreach (string imageFile in imageFiles)
            {
                JSONClass jc = SuperController.singleton.LoadJSON(prefabExample) as JSONClass;

                JSONClass o = Recursive(jc);
                if (o != null)
                {
                    //SuperController.LogError("FOUND KEY REC " + o["Path"] + " " +  imageFile);

                    o["Path"] = imageFile;

                    string imageName = GetFileNameWithoutExtension(imageFile);
                    imageName = imageName.Replace("_", "");

                    string newFile = string.Format("{0}/{1}.DecalMakerPreset.json", dirPath, imageName);

                    SuperController.singleton.SaveJSON(jc, newFile);

                    prefabs.Add(newFile);
                }
            }

            DM.StartCoroutine(ScreenShotTask(prefabs));
        }

        IEnumerator ScreenShotTask(List<string> prefabs)
        {
            foreach (string prefab in prefabs)
            {
                DM.PresetLoad(prefab, true);
                //wait for clothing and any texture loads are finished
                yield return new WaitWhile(() => SuperController.singleton.loadingIcon.gameObject.activeSelf);
                yield return new WaitForSeconds(1);
               
                SuperController.singleton.DoSaveScreenshot(prefab, ScreenShotCall);
                waitingOnScreenShot = true;
          
                SuperController.singleton.SetLeftSelect();
                yield return new WaitWhile(() => waitingOnScreenShot);
                DM.CallAction("ClearAll");
                yield return new WaitForSeconds(1);

            }

            
            yield break;
        }

        private void ScreenShotCall(string text)
        {
            waitingOnScreenShot = false;

        }


    }
}
