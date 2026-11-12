using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SandsOfLegends.UI
{
    /// <summary>
    /// Original project fantasy home landscape with named, tappable landmarks.
    /// Labels are mapped to familiar kingdom destinations while preserving working HomeScreen events.
    /// </summary>
    public sealed class HomeWorldScenery : MonoBehaviour
    {
        private const float LabelScreenFraction = 0.026f;
        private const float GroundRadius = 150f;
        private struct Hill { public Vector3 center; public Vector3 radii; }
        private sealed class Cloud { public Transform t; public float speed; }
        private sealed class Mover { public Transform t; public float spin; public float bobAmp; public float bobSpeed; public float baseY; public float phase; }
        private readonly List<Hill> hills = new List<Hill>();
        private readonly List<Cloud> clouds = new List<Cloud>();
        private readonly List<Mover> movers = new List<Mover>();
        private readonly List<Transform> labels = new List<Transform>();
        private readonly Dictionary<int, Material> materials = new Dictionary<int, Material>();
        private readonly Dictionary<int, Mesh> cones = new Dictionary<int, Mesh>();
        private Material templateMaterial;
        private Font labelFont;
        private Material labelMaterial;
        private HomeScreen home;
        private Transform groundRoot, treeRoot, mountainRoot, landmarkRoot, skyRoot;
        private static readonly Color Grass = new Color(0.50f, 0.78f, 0.36f);
        private static readonly Color Stone = new Color(0.86f, 0.80f, 0.68f);
        private static readonly Color Cream = new Color(0.95f, 0.88f, 0.70f);
        private static readonly Color Gold = new Color(0.96f, 0.78f, 0.30f);
        private static readonly Color Teal = new Color(0.16f, 0.62f, 0.62f);
        private static readonly Color Crimson = new Color(0.82f, 0.26f, 0.28f);
        private static readonly Color Wood = new Color(0.62f, 0.42f, 0.26f);

        public static HomeWorldScenery Create(Transform parent)
        {
            GameObject go = new GameObject("Home Scenery");
            go.transform.SetParent(parent, false);
            HomeWorldScenery scenery = go.AddComponent<HomeWorldScenery>();
            scenery.Build();
            return scenery;
        }

        private void Build()
        {
            groundRoot = NewRoot("Ground"); treeRoot = NewRoot("Trees"); mountainRoot = NewRoot("Mountains");
            landmarkRoot = NewRoot("Landmarks"); skyRoot = NewRoot("Sky");
            BuildGround(); BuildMountains(); BuildTrees(); BuildLandmarks(); BuildFloatingIslands(); BuildClouds();
            try { StaticBatchingUtility.Combine(treeRoot.gameObject); StaticBatchingUtility.Combine(mountainRoot.gameObject); }
            catch (System.Exception e) { Debug.LogWarning("Home scenery static batching skipped: " + e.Message); }
        }

        private Transform NewRoot(string name) { GameObject go = new GameObject(name); go.transform.SetParent(transform, false); return go.transform; }

        private void BuildGround()
        {
            Prim(PrimitiveType.Cylinder, groundRoot, new Vector3(0f, -0.26f, 30f), new Vector3(GroundRadius * 2f, 0.4f, GroundRadius * 2f), Grass, "Grass Plain");
            AddHill(new Vector3(0f, -1.5f, 32f), new Vector3(24f, 7f, 14f), new Color(0.46f, 0.76f, 0.32f));
            AddHill(new Vector3(-26f, -2f, 20f), new Vector3(16f, 6f, 12f), new Color(0.40f, 0.70f, 0.30f));
            AddHill(new Vector3(27f, -2f, 18f), new Vector3(16f, 5.5f, 11f), new Color(0.42f, 0.72f, 0.32f));
            AddHill(new Vector3(0f, -2f, 44f), new Vector3(32f, 6f, 10f), new Color(0.36f, 0.64f, 0.34f));
            AddHill(new Vector3(-30f, -2f, 46f), new Vector3(16f, 5f, 9f), new Color(0.34f, 0.62f, 0.34f));
            AddHill(new Vector3(32f, -2f, 46f), new Vector3(16f, 5f, 9f), new Color(0.34f, 0.62f, 0.34f));
        }

        private void AddHill(Vector3 center, Vector3 radii, Color color)
        {
            Hill h = new Hill(); h.center = center; h.radii = radii; hills.Add(h);
            Prim(PrimitiveType.Sphere, groundRoot, center, radii * 2f, color, "Hill");
        }

        private float GroundY(float x, float z)
        {
            float y = 0f;
            for (int i = 0; i < hills.Count; i++)
            {
                Hill h = hills[i]; float nx = (x - h.center.x) / h.radii.x; float nz = (z - h.center.z) / h.radii.z;
                float d = 1f - nx * nx - nz * nz;
                if (d > 0f) y = Mathf.Max(y, h.center.y + h.radii.y * Mathf.Sqrt(d));
            }
            return y;
        }

        private void BuildMountains()
        {
            Color near = new Color(0.46f, 0.55f, 0.68f), far = new Color(0.56f, 0.65f, 0.79f);
            float[,] list = { { -48f,66f,22f,18f }, { -24f,74f,30f,20f }, { 4f,80f,34f,24f }, { 30f,72f,26f,19f }, { 52f,66f,21f,17f }, { -70f,60f,18f,16f }, { 74f,60f,18f,16f }, { -40f,110f,40f,30f }, { -5f,122f,46f,34f }, { 38f,112f,40f,30f }, { -82f,100f,34f,26f }, { 84f,100f,34f,26f } };
            for (int i = 0; i < list.GetLength(0); i++) AddMountain(list[i,0], list[i,1], list[i,2], list[i,3], list[i,1] > 95f ? far : near, i * 17f);
        }

        private void AddMountain(float x, float z, float h, float r, Color rock, float yaw)
        {
            float total = h + 1f; ConeObj(mountainRoot, 7, new Vector3(x,-1f,z), new Vector3(r*2f,total,r*2f), rock, yaw);
            const float f = 0.62f; float capR = r * (1f - f) * 1.06f;
            ConeObj(mountainRoot, 7, new Vector3(x,-1f+total*f,z), new Vector3(capR*2f,total*(1f-f),capR*2f), new Color(0.97f,0.98f,1f), yaw);
        }

        private void BuildTrees()
        {
            System.Random rng = new System.Random(7);
            Color[] greens = { new Color(.18f,.52f,.28f), new Color(.26f,.60f,.30f), new Color(.22f,.56f,.34f) };
            int placed = 0;
            for (int attempt = 0; attempt < 400 && placed < 46; attempt++)
            {
                float x = (float)(rng.NextDouble()*70.0-35.0), z = (float)(rng.NextDouble()*38.0+12.0);
                if (Mathf.Abs(x)<8f && z<20f || TooCloseToLandmark(x,z,6f)) continue;
                float y = GroundY(x,z); if (y<.4f) continue;
                float s = .8f + (float)rng.NextDouble()*.8f; Color c = greens[rng.Next(greens.Length)];
                if (rng.NextDouble()<.65) Pine(new Vector3(x,y-.1f,z),s,c); else RoundTree(new Vector3(x,y-.1f,z),s,c);
                placed++;
            }
        }

        private static readonly Vector2[] LandmarkXZ = { new Vector2(-9f,24f),new Vector2(9f,24f),new Vector2(-4.5f,31f),new Vector2(4.5f,31f),new Vector2(0f,41f),new Vector2(13f,40f) };
        private bool TooCloseToLandmark(float x,float z,float radius) { for(int i=0;i<LandmarkXZ.Length;i++) if((new Vector2(x,z)-LandmarkXZ[i]).sqrMagnitude<radius*radius) return true; return false; }

        private void Pine(Vector3 pos,float s,Color leaf)
        {
            Transform t=new GameObject("Pine").transform; t.SetParent(treeRoot,false); t.localPosition=pos; t.localScale=Vector3.one*s;
            Prim(PrimitiveType.Cylinder,t,new Vector3(0f,.7f,0f),new Vector3(.45f,.7f,.45f),Wood,"Trunk");
            ConeObj(t,7,new Vector3(0f,1f,0f),new Vector3(2.6f,2.2f,2.6f),leaf,0f); ConeObj(t,7,new Vector3(0f,2.2f,0f),new Vector3(2f,2f,2f),leaf*1.08f,0f); ConeObj(t,7,new Vector3(0f,3.4f,0f),new Vector3(1.4f,1.7f,1.4f),leaf*1.16f,0f);
        }
        private void RoundTree(Vector3 pos,float s,Color leaf)
        {
            Transform t=new GameObject("Round Tree").transform; t.SetParent(treeRoot,false); t.localPosition=pos; t.localScale=Vector3.one*s;
            Prim(PrimitiveType.Cylinder,t,new Vector3(0f,.9f,0f),new Vector3(.5f,.9f,.5f),Wood,"Trunk"); Prim(PrimitiveType.Sphere,t,new Vector3(0f,2.6f,0f),new Vector3(2.8f,2.5f,2.8f),leaf*1.1f,"Crown"); Prim(PrimitiveType.Sphere,t,new Vector3(.9f,2.1f,.3f),new Vector3(1.8f,1.6f,1.8f),leaf,"Crown Side");
        }

        private void BuildLandmarks()
        {
            BuildSummonShrine(LandmarkXZ[0]); BuildArena(LandmarkXZ[1]); BuildHeroHall(LandmarkXZ[2]); BuildMarket(LandmarkXZ[3]); BuildCampaignGate(LandmarkXZ[4]); BuildBossSpire(LandmarkXZ[5]);
        }
        private Transform Landmark(string key,string title,Vector2 xz,float topY)
        {
            float y=GroundY(xz.x,xz.y); GameObject go=new GameObject("Landmark - "+title); go.transform.SetParent(landmarkRoot,false); go.transform.localPosition=new Vector3(xz.x,y-.15f,xz.y);
            HomeLandmark lm=go.AddComponent<HomeLandmark>(); lm.key=key; BoxCollider box=go.AddComponent<BoxCollider>(); box.center=new Vector3(0f,(topY+2f)*.5f,0f); box.size=new Vector3(8f,topY+2f,8f);
            Prim(PrimitiveType.Cylinder,go.transform,new Vector3(0f,.05f,0f),new Vector3(7f,.2f,7f),Stone,"Plaza"); AddLabel(go.transform,title,topY+1.4f); return go.transform;
        }
        private void BuildSummonShrine(Vector2 xz)
        {
            Transform t=Landmark("summon","SUMMON",xz,5.4f); Prim(PrimitiveType.Cylinder,t,new Vector3(0f,.4f,0f),new Vector3(3.8f,.25f,3.8f),Stone,"Dais Low"); Prim(PrimitiveType.Cylinder,t,new Vector3(0f,.8f,0f),new Vector3(2.6f,.25f,2.6f),Cream,"Dais High");
            for(int sx=-1;sx<=1;sx+=2) for(int sz=-1;sz<=1;sz+=2) { Prim(PrimitiveType.Cylinder,t,new Vector3(sx*1.7f,2.1f,sz*1.7f),new Vector3(.4f,1.3f,.4f),Cream,"Pillar"); Prim(PrimitiveType.Cube,t,new Vector3(sx*1.7f,3.5f,sz*1.7f),new Vector3(.7f,.2f,.7f),Gold,"Pillar Cap"); }
            Transform ring=Prim(PrimitiveType.Cylinder,t,new Vector3(0f,3.4f,0f),new Vector3(3.3f,.06f,3.3f),Gold,"Gold Ring"); AddMover(ring,25f,0f,0f); Gem(t,new Vector3(0f,3.4f,0f),1.4f,1.8f,new Color(.45f,.85f,1f));
        }
        private void BuildArena(Vector2 xz)
        {
            Transform t=Landmark("arena","PVP",xz,6f); Prim(PrimitiveType.Cylinder,t,new Vector3(0f,1.3f,0f),new Vector3(6.4f,1.3f,6.4f),new Color(.93f,.84f,.66f),"Arena Wall"); Prim(PrimitiveType.Cylinder,t,new Vector3(0f,2.65f,0f),new Vector3(6.7f,.2f,6.7f),Gold,"Gold Rim"); Prim(PrimitiveType.Cylinder,t,new Vector3(0f,2.5f,0f),new Vector3(5.4f,.2f,5.4f),new Color(.80f,.66f,.44f),"Sand Floor");
            for(int k=0;k<8;k++) { float a=k*Mathf.PI*.25f; Vector3 p=new Vector3(Mathf.Cos(a)*3f,0f,Mathf.Sin(a)*3f); Prim(PrimitiveType.Cylinder,t,p+new Vector3(0f,3.4f,0f),new Vector3(.35f,.75f,.35f),Cream,"Arena Pillar"); Prim(PrimitiveType.Cube,t,p+new Vector3(0f,4.2f,0f),new Vector3(.6f,.18f,.6f),Gold,"Pillar Cap"); if(k%2==0) { Prim(PrimitiveType.Cylinder,t,p+new Vector3(0f,4.9f,0f),new Vector3(.06f,.7f,.06f),Wood,"Flag Pole"); Prim(PrimitiveType.Cube,t,p+new Vector3(.3f,5.3f,0f),new Vector3(.6f,.35f,.05f),Crimson,"Flag"); } }
        }
        private void BuildHeroHall(Vector2 xz)
        {
            Transform t=Landmark("heroes","GUILD",xz,6.3f); Prim(PrimitiveType.Cube,t,new Vector3(0f,1.55f,0f),new Vector3(3.7f,2.6f,3.7f),new Color(.93f,.86f,.72f),"Hall Body"); ConeObj(t,4,new Vector3(0f,2.85f,0f),new Vector3(5.2f,2f,5.2f),Teal,45f); Prim(PrimitiveType.Cube,t,new Vector3(0f,3.8f,0f),new Vector3(2.4f,1.4f,2.4f),new Color(.90f,.80f,.64f),"Hall Tower"); ConeObj(t,4,new Vector3(0f,4.5f,0f),new Vector3(3.4f,1.8f,3.4f),new Color(.12f,.50f,.55f),45f); Prim(PrimitiveType.Cylinder,t,new Vector3(0f,6f,0f),new Vector3(.05f,.5f,.05f),Wood,"Pennant Pole"); Prim(PrimitiveType.Cube,t,new Vector3(.28f,6.3f,0f),new Vector3(.55f,.3f,.05f),Gold,"Pennant"); Prim(PrimitiveType.Cube,t,new Vector3(0f,.9f,-1.88f),new Vector3(.9f,1.3f,.12f),new Color(.38f,.24f,.15f),"Door"); Prim(PrimitiveType.Cube,t,new Vector3(1.3f,1.9f,-1.88f),new Vector3(.5f,1.4f,.08f),Crimson,"Banner"); Prim(PrimitiveType.Cube,t,new Vector3(-1.3f,1.9f,-1.88f),new Vector3(.5f,1.4f,.08f),Teal,"Banner");
        }
        private void BuildMarket(Vector2 xz)
        {
            Transform t=Landmark("shop","SHOP",xz,4.7f); Prim(PrimitiveType.Cube,t,new Vector3(0f,1.3f,0f),new Vector3(3.1f,2.1f,3.1f),Cream,"Shop Body"); ConeObj(t,4,new Vector3(0f,2.35f,0f),new Vector3(4.6f,1.9f,4.6f),Crimson,45f); Prim(PrimitiveType.Cube,t,new Vector3(0f,.7f,-2f),new Vector3(2.2f,.7f,.9f),Wood,"Counter"); Transform sign=Prim(PrimitiveType.Cylinder,t,new Vector3(0f,1.9f,-1.62f),new Vector3(.9f,.06f,.9f),Gold,"Coin Sign"); sign.localRotation=Quaternion.Euler(90f,0f,0f); Prim(PrimitiveType.Cylinder,t,new Vector3(-1.9f,.5f,-1.6f),new Vector3(.5f,.45f,.5f),Wood,"Barrel"); Prim(PrimitiveType.Cylinder,t,new Vector3(1.9f,.5f,-1.6f),new Vector3(.5f,.45f,.5f),Wood,"Barrel");
        }
        private void BuildCampaignGate(Vector2 xz)
        {
            Transform t=Landmark("campaign","CONTINENT",xz,6.4f); for(int s=-1;s<=1;s+=2) { Prim(PrimitiveType.Cylinder,t,new Vector3(s*2.3f,2.8f,0f),new Vector3(1f,2.6f,1f),Cream,"Gate Pillar"); Prim(PrimitiveType.Cylinder,t,new Vector3(s*2.3f,5.5f,0f),new Vector3(1.3f,.18f,1.3f),Gold,"Pillar Cap"); ConeObj(t,8,new Vector3(s*2.3f,5.6f,0f),new Vector3(1.1f,1f,1.1f),Teal,0f); } Prim(PrimitiveType.Cube,t,new Vector3(0f,5.1f,0f),new Vector3(5.6f,.9f,1.1f),Cream,"Lintel"); Prim(PrimitiveType.Cube,t,new Vector3(0f,5.6f,0f),new Vector3(5.7f,.2f,1.2f),Gold,"Lintel Gold"); Transform portal=Prim(PrimitiveType.Cylinder,t,new Vector3(0f,2.9f,0f),new Vector3(3.8f,.05f,4.6f),new Color(.55f,.95f,1f),"Portal"); portal.localRotation=Quaternion.Euler(90f,0f,0f); Transform core=Prim(PrimitiveType.Cylinder,t,new Vector3(0f,2.9f,-.06f),new Vector3(2.4f,.06f,3f),new Color(.88f,1f,1f),"Portal Core"); core.localRotation=Quaternion.Euler(90f,0f,0f); Prim(PrimitiveType.Cube,t,new Vector3(0f,.35f,-1.6f),new Vector3(5.4f,.3f,1.6f),Stone,"Steps");
        }
        private void BuildBossSpire(Vector2 xz)
        {
            Transform t=Landmark("boss","DUNGEON",xz,12f); Prim(PrimitiveType.Cylinder,t,new Vector3(0f,3.8f,0f),new Vector3(2.6f,3.6f,2.6f),new Color(.32f,.28f,.45f),"Spire Body"); Prim(PrimitiveType.Cylinder,t,new Vector3(0f,6f,0f),new Vector3(2.8f,.12f,2.8f),Gold,"Spire Band"); Prim(PrimitiveType.Cylinder,t,new Vector3(0f,7.6f,0f),new Vector3(3.4f,.35f,3.4f),new Color(.55f,.16f,.20f),"Spire Gallery"); ConeObj(t,8,new Vector3(0f,7.9f,0f),new Vector3(3.6f,3.2f,3.6f),new Color(.62f,.14f,.22f),0f); for(int s=-1;s<=1;s+=2) Prim(PrimitiveType.Cube,t,new Vector3(s*1.7f,2f,0f),new Vector3(.5f,3f,1.2f),new Color(.24f,.20f,.34f),"Spire Fin"); Gem(t,new Vector3(0f,11.9f,0f),.9f,1.2f,new Color(1f,.55f,.20f));
        }

        private void BuildFloatingIslands() { AddIslet(new Vector3(-20f,14f,50f),5f); AddIslet(new Vector3(20f,18f,56f),4.2f); AddIslet(new Vector3(2f,26f,66f),5.6f); }
        private void AddIslet(Vector3 pos,float radius)
        {
            Transform root=new GameObject("Floating Islet").transform; root.SetParent(skyRoot,false); root.localPosition=pos; Prim(PrimitiveType.Cylinder,root,Vector3.zero,new Vector3(radius*2f,.25f,radius*2f),new Color(.50f,.80f,.36f),"Islet Grass"); ConeObj(root,6,Vector3.zero,new Vector3(radius*1.9f,radius*1.5f,radius*1.9f),new Color(.55f,.45f,.38f),0f).localRotation=Quaternion.Euler(180f,0f,0f); Transform tree=new GameObject("Islet Tree").transform; tree.SetParent(root,false); tree.localPosition=new Vector3(radius*.25f,.2f,0f); tree.localScale=Vector3.one*(radius*.28f); Prim(PrimitiveType.Cylinder,tree,new Vector3(0f,.7f,0f),new Vector3(.45f,.7f,.45f),Wood,"Trunk"); ConeObj(tree,7,new Vector3(0f,1f,0f),new Vector3(2.6f,2.2f,2.6f),new Color(.20f,.56f,.30f),0f); ConeObj(tree,7,new Vector3(0f,2.2f,0f),new Vector3(2f,2f,2f),new Color(.24f,.62f,.32f),0f); AddMover(root,0f,.7f,.6f);
        }
        private void BuildClouds()
        {
            System.Random rng=new System.Random(21); for(int i=0;i<14;i++) { float x=(float)(rng.NextDouble()*180.0-90.0), y=16f+(float)rng.NextDouble()*24f, z=55f+(float)rng.NextDouble()*60f, s=.8f+(float)rng.NextDouble()*.9f; AddCloud(new Vector3(x,y,z),s,.35f+(float)rng.NextDouble()*.6f,1f); } for(int i=0;i<6;i++) { float x=(float)(rng.NextDouble()*120.0-60.0), y=5f+(float)rng.NextDouble()*5f, z=56f+(float)rng.NextDouble()*10f; AddCloud(new Vector3(x,y,z),1.1f+(float)rng.NextDouble()*.6f,.2f+(float)rng.NextDouble()*.3f,.55f); }
        }
        private void AddCloud(Vector3 pos,float scale,float speed,float flatten)
        {
            Transform root=new GameObject("Cloud").transform; root.SetParent(skyRoot,false); root.localPosition=pos; Color white=Color.white; Prim(PrimitiveType.Sphere,root,Vector3.zero,new Vector3(9f,3f*flatten,5f)*scale,white,"Puff"); Prim(PrimitiveType.Sphere,root,new Vector3(4f,.4f,0f)*scale,new Vector3(6f,2.6f*flatten,4f)*scale,white,"Puff"); Prim(PrimitiveType.Sphere,root,new Vector3(-4.5f,.2f,.5f)*scale,new Vector3(6.5f,2.4f*flatten,4f)*scale,white,"Puff"); Prim(PrimitiveType.Sphere,root,new Vector3(1f,1.2f*flatten,0f)*scale,new Vector3(5f,2.6f*flatten,3.6f)*scale,white,"Puff"); clouds.Add(new Cloud { t=root, speed=speed });
        }

        private Transform Gem(Transform parent,Vector3 pos,float width,float halfHeight,Color color)
        {
            Transform pivot=new GameObject("Gem").transform; pivot.SetParent(parent,false); pivot.localPosition=pos; ConeObj(pivot,6,Vector3.zero,new Vector3(width,halfHeight,width),color,0f); ConeObj(pivot,6,Vector3.zero,new Vector3(width,halfHeight,width),color*.8f,0f).localRotation=Quaternion.Euler(180f,0f,0f); AddMover(pivot,45f,.25f,1.6f); return pivot;
        }
        private void AddMover(Transform t,float spin,float bobAmp,float bobSpeed) { movers.Add(new Mover { t=t,spin=spin,bobAmp=bobAmp,bobSpeed=bobSpeed,baseY=t.localPosition.y,phase=Random.value*6.28f }); }
        private void AddLabel(Transform parent,string text,float y)
        {
            if(labelFont==null) { try { labelFont=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch(System.ArgumentException) { } if(labelFont==null) { try { labelFont=Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch(System.ArgumentException) { } } if(labelFont!=null) labelMaterial=labelFont.material; }
            if(labelFont==null) return; GameObject root=new GameObject("Label"); root.transform.SetParent(parent,false); root.transform.localPosition=new Vector3(0f,y,0f); labels.Add(root.transform); MakeText(root.transform,text,new Color(.05f,.08f,.12f,.95f),new Vector3(.05f,-.05f,.02f),0); MakeText(root.transform,text,new Color(1f,.94f,.79f,1f),Vector3.zero,1);
        }
        private void MakeText(Transform parent,string text,Color color,Vector3 localPos,int order)
        {
            GameObject go=new GameObject("Text"); go.transform.SetParent(parent,false); go.transform.localPosition=localPos; TextMesh tm=go.AddComponent<TextMesh>(); tm.font=labelFont; tm.text=text; tm.fontSize=64; tm.characterSize=.1f; tm.anchor=TextAnchor.MiddleCenter; tm.alignment=TextAlignment.Center; tm.fontStyle=FontStyle.Bold; tm.color=color; MeshRenderer mr=go.GetComponent<MeshRenderer>(); mr.sharedMaterial=labelMaterial; mr.sortingOrder=order; mr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows=false;
        }
        private Material Mat(Color c)
        {
            int key=(Mathf.Clamp(Mathf.RoundToInt(c.r*255f),0,255)<<16)|(Mathf.Clamp(Mathf.RoundToInt(c.g*255f),0,255)<<8)|Mathf.Clamp(Mathf.RoundToInt(c.b*255f),0,255); Material m; if(materials.TryGetValue(key,out m)) return m; if(templateMaterial==null) { GameObject probe=GameObject.CreatePrimitive(PrimitiveType.Cube); templateMaterial=probe.GetComponent<Renderer>().sharedMaterial; Destroy(probe); } m=new Material(templateMaterial); m.color=new Color(Mathf.Clamp01(c.r),Mathf.Clamp01(c.g),Mathf.Clamp01(c.b),1f); if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",.08f); if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f); materials[key]=m; return m;
        }
        private Transform Prim(PrimitiveType type,Transform parent,Vector3 pos,Vector3 scale,Color color,string name)
        {
            GameObject go=GameObject.CreatePrimitive(type); go.name=name; Collider col=go.GetComponent<Collider>(); if(col!=null)Destroy(col); go.transform.SetParent(parent,false); go.transform.localPosition=pos; go.transform.localScale=scale; Renderer r=go.GetComponent<Renderer>(); r.sharedMaterial=Mat(color); r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows=false; return go.transform;
        }
        private Transform ConeObj(Transform parent,int sides,Vector3 pos,Vector3 scale,Color color,float yaw)
        {
            GameObject go=new GameObject("Cone"); go.transform.SetParent(parent,false); go.transform.localPosition=pos; go.transform.localRotation=Quaternion.Euler(0f,yaw,0f); go.transform.localScale=scale; go.AddComponent<MeshFilter>().sharedMesh=ConeMesh(sides); MeshRenderer mr=go.AddComponent<MeshRenderer>(); mr.sharedMaterial=Mat(color); mr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows=false; return go.transform;
        }
        private Mesh ConeMesh(int sides)
        {
            Mesh mesh; if(cones.TryGetValue(sides,out mesh))return mesh; List<Vector3> v=new List<Vector3>(); List<int> tri=new List<int>(); Vector3 apex=new Vector3(0f,1f,0f); for(int i=0;i<sides;i++) { float a0=2f*Mathf.PI*i/sides,a1=2f*Mathf.PI*(i+1)/sides; Vector3 p0=new Vector3(Mathf.Cos(a0)*.5f,0f,Mathf.Sin(a0)*.5f),p1=new Vector3(Mathf.Cos(a1)*.5f,0f,Mathf.Sin(a1)*.5f); int s=v.Count; v.Add(apex);v.Add(p1);v.Add(p0);tri.Add(s);tri.Add(s+1);tri.Add(s+2);int b=v.Count;v.Add(Vector3.zero);v.Add(p0);v.Add(p1);tri.Add(b);tri.Add(b+1);tri.Add(b+2); } mesh=new Mesh();mesh.name="HomeCone"+sides;mesh.SetVertices(v);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateBounds();cones[sides]=mesh;return mesh;
        }
        private void Update()
        {
            float dt=Time.deltaTime; for(int i=0;i<clouds.Count;i++) { Transform t=clouds[i].t; Vector3 p=t.localPosition; p.x+=clouds[i].speed*dt; if(p.x>100f)p.x=-100f; t.localPosition=p; }
            for(int i=0;i<movers.Count;i++) { Mover m=movers[i]; if(m.t==null)continue; if(Mathf.Abs(m.spin)>.001f)m.t.Rotate(0f,m.spin*dt,0f,Space.Self); if(m.bobAmp>.001f) { Vector3 p=m.t.localPosition;p.y=m.baseY+Mathf.Sin(Time.time*m.bobSpeed+m.phase)*m.bobAmp;m.t.localPosition=p; } } HandleTap();
        }
        private void LateUpdate()
        {
            Camera cam=Camera.main;if(cam==null)return;float scalePerUnit=2f*Mathf.Tan(cam.fieldOfView*.5f*Mathf.Deg2Rad)*LabelScreenFraction;for(int i=0;i<labels.Count;i++){Transform l=labels[i];if(l==null)continue;l.rotation=cam.transform.rotation;float d=Vector3.Distance(cam.transform.position,l.position);l.localScale=Vector3.one*(d*scalePerUnit);}
        }
        private void HandleTap()
        {
            if(!Input.GetMouseButtonDown(0))return;Camera cam=Camera.main;if(cam==null)return;if(EventSystem.current!=null){bool overUi=Input.touchCount>0?EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId):EventSystem.current.IsPointerOverGameObject();if(overUi)return;}RaycastHit hit;if(!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition),out hit,400f))return;HomeLandmark lm=hit.collider.GetComponentInParent<HomeLandmark>();if(lm!=null)OpenLandmark(lm.key);
        }
        private void OpenLandmark(string key)
        {
            if(home==null)home=FindObjectOfType<HomeScreen>();if(home==null)return;switch(key){case "summon":home.onSummon.Invoke();break;case "arena":home.onArena.Invoke();break;case "shop":home.onShop.Invoke();break;case "heroes":home.onHeroes.Invoke();break;case "campaign":home.onCampaign.Invoke();break;case "boss":home.onAllianceBoss.Invoke();break;}
        }
    }
}
