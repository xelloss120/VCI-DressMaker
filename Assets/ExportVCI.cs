using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VCI;
using VCIGLTF;

public class ExportVCI : MonoBehaviour
{
    public GameObject VRM;

    [SerializeField] InputField Title;
    [SerializeField] InputField Version;
    [SerializeField] InputField Author;
    [SerializeField] InputField Contact;
    [SerializeField] InputField Reference;
    [SerializeField] Dropdown License;
    [SerializeField] Text ExportVCI_Text;

    [SerializeField] Material Mat;
    [SerializeField, TextArea] string lua;

    void Start()
    {
        Screen.SetResolution(520, 220, false, 60);
    }

    public void Export()
    {
        var title = Title.text;
        var version = Version.text;
        var author = Author.text;
        var contact = Contact.text;
        var reference = Reference.text;
        var license = License.value;

        if (title == "" || author == "")
        {
            ExportVCI_Text.text = "必須の項目を入力してください";
            return;
        }
        else
        {
            ExportVCI_Text.text = "Export VCI";
        }

        // VCI基本設定
        VRM.name = title;
        var vci = VRM.AddComponent<VCIObject>();
        vci.Meta.title = title;
        vci.Meta.version = version;
        vci.Meta.author = author;
        vci.Meta.contactInformation = contact;
        vci.Meta.reference = reference;
        vci.Meta.modelDataLicenseType = (glTF_VCAST_vci_meta.LicenseType)license;

        // luaスクリプト
        var script = new VCIObject.Script
        {
            mimeType = ScriptMimeType.X_APPLICATION_LUA,
            targetEngine = TargetEngine.MoonSharp,
            source = lua
        };
        vci.Scripts.Add(script);

        // 不要部位の削除
        DestroyImmediate(VRM.transform.Find("Face").gameObject);
        DestroyImmediate(VRM.transform.Find("Hairs").gameObject);
        DestroyImmediate(VRM.transform.Find("secondary").gameObject);

        // 不要メッシュの透過
        var renderer = VRM.transform.Find("Body").gameObject.GetComponent<Renderer>();
        var materials = renderer.materials;
        for (int i = 0; i < renderer.materials.Length - 2; i++)
        {
            materials[i] = Mat;
        }
        // 後ろから二番目のマテリアルが服テクスチャの想定
        materials[renderer.materials.Length - 1] = Mat;
        renderer.materials = materials;

        // バージョン差吸収
        string hips = "Root/J_Bip_C_Hips/";
        if (VRM.transform.Find(hips) == null)
        {
            hips = "Root/Global/Position/J_Bip_C_Hips/";
        }

        // 足
        SetAtt(hips + "J_Bip_L_UpperLeg/J_Bip_L_LowerLeg/J_Bip_L_Foot", HumanBodyBones.LeftFoot);
        SetAtt(hips + "J_Bip_R_UpperLeg/J_Bip_R_LowerLeg/J_Bip_R_Foot", HumanBodyBones.RightFoot);
        SetAtt(hips + "J_Bip_L_UpperLeg/J_Bip_L_LowerLeg", HumanBodyBones.LeftLowerLeg);
        SetAtt(hips + "J_Bip_R_UpperLeg/J_Bip_R_LowerLeg", HumanBodyBones.RightLowerLeg);
        SetAtt(hips + "J_Bip_L_UpperLeg", HumanBodyBones.LeftUpperLeg);
        SetAtt(hips + "J_Bip_R_UpperLeg", HumanBodyBones.RightUpperLeg);

        // 腕
        string arm = hips + "J_Bip_C_Spine/J_Bip_C_Chest/J_Bip_C_UpperChest/";
        SetAtt(arm + "J_Bip_L_Shoulder/J_Bip_L_UpperArm/J_Bip_L_LowerArm/J_Bip_L_Hand", HumanBodyBones.LeftHand);
        SetAtt(arm + "J_Bip_R_Shoulder/J_Bip_R_UpperArm/J_Bip_R_LowerArm/J_Bip_R_Hand", HumanBodyBones.RightHand);
        SetAtt(arm + "J_Bip_L_Shoulder/J_Bip_L_UpperArm/J_Bip_L_LowerArm", HumanBodyBones.LeftLowerArm);
        SetAtt(arm + "J_Bip_R_Shoulder/J_Bip_R_UpperArm/J_Bip_R_LowerArm", HumanBodyBones.RightLowerArm);
        SetAtt(arm + "J_Bip_L_Shoulder/J_Bip_L_UpperArm", HumanBodyBones.LeftUpperArm);
        SetAtt(arm + "J_Bip_R_Shoulder/J_Bip_R_UpperArm", HumanBodyBones.RightUpperArm);

        // 体幹
        SetAtt(hips + "J_Bip_C_Spine/J_Bip_C_Chest", HumanBodyBones.Chest);
        SetAtt(hips + "J_Bip_C_Spine", HumanBodyBones.Spine);
        SetAtt(hips , HumanBodyBones.Hips);

        var gltf = new glTF();
        var exporter = new VCIExporter(gltf);
        exporter.Prepare(VRM);
        exporter.Export();
        var bytes = gltf.ToGlbBytes();
        var path = Application.dataPath + "/../" + title + ".vci";
        File.WriteAllBytes(path, bytes);

        Destroy(exporter.Copy);
        Destroy(VRM);
    }

    /// <summary>
    /// VRoidボーンのパスと人型ボーン定義を紐づけて必要設定を適用
    /// </summary>
    /// <param name="path">VRoidボーンのパス</param>
    /// <param name="bone">人型ボーン定義</param>
    void SetAtt(string path, HumanBodyBones bone)
    {
        // 全部位設定
        var obj = VRM.transform.Find(path).gameObject;
        var sub = obj.AddComponent<VCISubItem>();
        var rig = obj.AddComponent<Rigidbody>();
        var att = obj.AddComponent<VCIAttachable>();
        var box = obj.AddComponent<BoxCollider>();
        obj.transform.parent = VRM.transform;
        sub.Grabbable = true;
        sub.Scalable = true;
        sub.GroupId = 1;
        rig.useGravity = false;
        rig.isKinematic = true;
        att.AttachableHumanBodyBones = new HumanBodyBones[] { bone };
        att.AttachableDistance = 0.05f;
        box.size = Vector3.one * 0.1f;
        box.isTrigger = true;

        // 個別部位設定
        if (bone == HumanBodyBones.LeftHand || bone == HumanBodyBones.RightHand)
        {
            box.size = Vector3.zero;
        }
        if (bone == HumanBodyBones.Chest)
        {
            box.size = Vector3.one * 0.2f;
            box.center = Vector3.up * 0.1f;
        }
    }
}
