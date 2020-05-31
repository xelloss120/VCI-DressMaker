using System.IO;
using System.Collections.Generic;
using UnityEngine;
using VRM;
using B83.Win32;

public class DandD : MonoBehaviour
{
    [SerializeField] GameObject Description;
    [SerializeField] ExportVCI ExportVCI;

    GameObject VRM;

    void OnEnable()
    {
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;
    }

    void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    void ImportVRM(string path)
    {
        if (VRM != null)
        {
            Destroy(VRM);
        }

        var context = new VRMImporterContext();

        context.Parse(path);
        context.LoadAsync(() =>
        {
            var gameObject = context.Root;
            gameObject.transform.position = new Vector3(0, 0, 0);
            gameObject.transform.eulerAngles = new Vector3(0, 180, 0);

            context.ShowMeshes(); // メッシュの表示・ここ重要

            ExportVCI.VRM = gameObject;

            Description.SetActive(false);

            VRM = gameObject;
        });
    }

    void OnFiles(List<string> aFiles, POINT aPos)
    {
        foreach (string path in aFiles)
        {
            var ext = Path.GetExtension(path);
            if (string.Compare(ext, ".vrm", true) == 0)
            {
                ImportVRM(path);
            }
        }
    }

    void Start()
    {
#if UNITY_EDITOR
        //ImportVRM(@"C:\test.vrm");
#endif
    }
}
