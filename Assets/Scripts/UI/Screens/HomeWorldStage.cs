// Procedural visual greybox for the Home hub. Replace with approved licensed art before final art sign-off.
using UnityEngine;

namespace SandsOfLegends.UI
{
    public sealed class HomeWorldStage : MonoBehaviour
    {
        private Transform root;
        private Material stone, shadow, sand, gold, teal, red, steel, skin;

        private void Awake()
        {
            stone=Mat(new Color(.62f,.53f,.39f)); shadow=Mat(new Color(.17f,.23f,.24f));
            sand=Mat(new Color(.72f,.56f,.34f)); gold=Mat(new Color(.92f,.68f,.25f),.25f);
            teal=Mat(new Color(.035f,.39f,.38f)); red=Mat(new Color(.48f,.12f,.13f));
            steel=Mat(new Color(.30f,.40f,.43f),.45f); skin=Mat(new Color(.72f,.49f,.34f));
            root=new GameObject("Procedural Castle Greybox").transform; root.SetParent(transform,false);
            Ground(); Castle(); Hero(); Torches(); CameraSetup();
        }

        private Material Mat(Color c,float metallic=0f)
        {
            Material m=new Material(Shader.Find("Standard")); m.color=c;
            m.SetFloat("_Metallic",metallic); m.SetFloat("_Glossiness",.28f); return m;
        }

        private void Part(PrimitiveType type,string name,Vector3 p,Vector3 s,Material m,Vector3 rotation=default(Vector3))
        {
            GameObject o=GameObject.CreatePrimitive(type); o.name=name; o.transform.SetParent(root,false);
            o.transform.localPosition=p; o.transform.localScale=s; o.transform.localEulerAngles=rotation;
            Renderer r=o.GetComponent<Renderer>(); r.sharedMaterial=m;
            Collider c=o.GetComponent<Collider>(); if(c!=null) Destroy(c);
        }

        private void Ground()
        {
            Part(PrimitiveType.Cube,"Sandstone Courtyard",new Vector3(0,-.28f,0),new Vector3(18,.5f,22),sand);
            Part(PrimitiveType.Cylinder,"Hero Dais",new Vector3(0,.02f,-2.2f),new Vector3(3,.18f,3),gold);
            Part(PrimitiveType.Cylinder,"Dais Top",new Vector3(0,.14f,-2.2f),new Vector3(2.72f,.12f,2.72f),shadow);
            for(int i=0;i<6;i++) Part(PrimitiveType.Cube,"Courtyard Stone",new Vector3(-6+i*2.4f,.01f,1.4f),new Vector3(1.7f,.04f,.08f),stone);
        }

        private void Castle()
        {
            Part(PrimitiveType.Cube,"Main Curtain Wall",new Vector3(0,3.1f,5.6f),new Vector3(13,6.2f,1.1f),stone);
            Part(PrimitiveType.Cube,"Gate Recess",new Vector3(0,1.55f,4.98f),new Vector3(3.8f,3.4f,.13f),shadow);
            Part(PrimitiveType.Cube,"Gate Left Leaf",new Vector3(-.86f,1.5f,4.86f),new Vector3(1.55f,3,.2f),teal);
            Part(PrimitiveType.Cube,"Gate Right Leaf",new Vector3(.86f,1.5f,4.86f),new Vector3(1.55f,3,.2f),teal);
            Part(PrimitiveType.Cube,"Golden Gate Lintel",new Vector3(0,3.3f,4.8f),new Vector3(4.2f,.42f,.35f),gold);
            for(int side=-1;side<=1;side+=2)
            {
                float x=side*5.7f;
                Part(PrimitiveType.Cylinder,"Round Watchtower",new Vector3(x,3.5f,5f),new Vector3(2.15f,7f,2.15f),shadow);
                Part(PrimitiveType.Cylinder,"Tower Crown",new Vector3(x,7.25f,5f),new Vector3(2.55f,.55f,2.55f),gold);
                Part(PrimitiveType.Cube,"Tower Parapet",new Vector3(x,7.85f,5f),new Vector3(2.4f,.75f,2.4f),stone);
                Part(PrimitiveType.Cube,"Side Wall",new Vector3(side*3.1f,2.4f,5.55f),new Vector3(3.1f,4.8f,.9f),stone);
                Part(PrimitiveType.Cube,"Banner Pole",new Vector3(x,7.4f,3.75f),new Vector3(.08f,3.1f,.08f),gold);
                Part(PrimitiveType.Cube,"Faction Banner",new Vector3(x-side*.48f,7.05f,3.75f),new Vector3(.95f,1.6f,.08f),side<0?teal:red);
                Part(PrimitiveType.Sphere,"Banner Emblem",new Vector3(x-side*.48f,7.12f,3.65f),new Vector3(.32f,.32f,.1f),gold);
            }
            for(int i=0;i<7;i++) Part(PrimitiveType.Cube,"Wall Merlon",new Vector3(-5.4f+i*1.8f,6.45f,5.05f),new Vector3(.62f,.55f,.95f),stone);
            Part(PrimitiveType.Cube,"Central Keep",new Vector3(0,7f,6.35f),new Vector3(4,2,1.8f),stone);
            Part(PrimitiveType.Cylinder,"Keep Roof",new Vector3(0,8.45f,6.35f),new Vector3(3.1f,1.4f,2.2f),teal,new Vector3(0,0,0));
        }

        private void Hero()
        {
            Part(PrimitiveType.Capsule,"Hero Body Placeholder",new Vector3(0,1.22f,-2.2f),new Vector3(.78f,1.1f,.48f),steel);
            Part(PrimitiveType.Sphere,"Hero Head Placeholder",new Vector3(0,2.58f,-2.2f),new Vector3(.48f,.55f,.46f),skin);
            Part(PrimitiveType.Cube,"Chestplate",new Vector3(0,1.55f,-2.12f),new Vector3(.9f,.72f,.18f),gold);
            Part(PrimitiveType.Capsule,"Left Greave",new Vector3(-.27f,.42f,-2.2f),new Vector3(.27f,.7f,.3f),shadow);
            Part(PrimitiveType.Capsule,"Right Greave",new Vector3(.27f,.42f,-2.2f),new Vector3(.27f,.7f,.3f),shadow);
            Part(PrimitiveType.Cube,"Shield",new Vector3(-.82f,1.25f,-2.1f),new Vector3(.62f,.92f,.16f),teal,new Vector3(0,0,-12));
            Part(PrimitiveType.Sphere,"Shield Crest",new Vector3(-.82f,1.28f,-2f),new Vector3(.22f,.22f,.08f),gold);
            Part(PrimitiveType.Cube,"Sword Blade",new Vector3(.8f,1.58f,-2.1f),new Vector3(.13f,1.05f,.12f),gold,new Vector3(0,0,-12));
            Part(PrimitiveType.Cube,"Sword Hilt",new Vector3(.9f,1.02f,-2.1f),new Vector3(.4f,.12f,.15f),shadow,new Vector3(0,0,-12));
            Part(PrimitiveType.Capsule,"Cape",new Vector3(0,1.35f,-2.48f),new Vector3(.7f,1.1f,.12f),red);
        }

        private void Torches()
        {
            for(int side=-1;side<=1;side+=2)
            {
                Vector3 p=new Vector3(side*3f,1.1f,.9f);
                Part(PrimitiveType.Cylinder,"Torch Stand",p,new Vector3(.12f,1.1f,.12f),shadow);
                Part(PrimitiveType.Sphere,"Torch Flame",p+Vector3.up*.7f,new Vector3(.3f,.42f,.3f),gold);
                GameObject go=new GameObject("Torch Point Light"); go.transform.SetParent(root,false); go.transform.localPosition=p+Vector3.up*.8f;
                Light l=go.AddComponent<Light>(); l.type=LightType.Point; l.color=new Color(1f,.62f,.28f); l.intensity=2f; l.range=7f;
            }
        }

        private void CameraSetup()
        {
            Camera cam=Camera.main;
            if(cam==null){GameObject go=new GameObject("Home World Camera",typeof(Camera)); cam=go.GetComponent<Camera>(); go.tag="MainCamera";}
            cam.transform.position=new Vector3(0,5.2f,-12.5f); cam.transform.LookAt(new Vector3(0,3.4f,3.2f));
            cam.fieldOfView=42f; cam.nearClipPlane=.1f; cam.farClipPlane=100f;
            cam.clearFlags=CameraClearFlags.SolidColor; cam.backgroundColor=new Color(.055f,.12f,.15f);
            RenderSettings.ambientLight=new Color(.48f,.49f,.46f);
            Light[] lights = FindObjectsOfType<Light>(); bool hasDirectional = false; for (int i=0;i<lights.Length;i++) if (lights[i].type == LightType.Directional) hasDirectional = true; if(!hasDirectional){GameObject sun=new GameObject("Home Directional Light"); Light l=sun.AddComponent<Light>(); l.type=LightType.Directional; l.intensity=1.15f; sun.transform.rotation=Quaternion.Euler(42,-28,0);}
        }
    }
}
