using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter)),RequireComponent(typeof(MeshRenderer)),RequireComponent(typeof(PolygonCollider2D))]
[ExecuteInEditMode]
public class ClothSprite:MonoBehaviour{

	#region Variables and classes

	[SerializeField]
	private MeshRenderer mr;
	[SerializeField]
	private Material mat;
	[SerializeField]
	private MeshFilter mf;
	[SerializeField]
	private PolygonCollider2D col;
	[SerializeField]
	private Mesh mesh;
	[SerializeField]
	private List<Vector3> vertices=new List<Vector3>(200);
	[SerializeField]
	private List<Vector3> uvs=new List<Vector3>(200);
	[SerializeField]
	private List<Color> colors=new List<Color>(200);
	[SerializeField]
	private List<Vector3> normals=new List<Vector3>(200);
	[SerializeField]
	private int[] triangles;
	[SerializeField]
	private int trianglesCount;

	[System.Serializable]
	public struct Point{
		public Vector3 position;
		public Vector3 prevPosition;
		public Vector3 acceleration;
		public float isFixed;
	}
	public Point[,] points;
	public Vector2[] colPoints=new Vector2[8];

	[System.Serializable]
	private struct Constraint{
		public int x1;
		public int y1;
		public int x2;
		public int y2;
	}
	[SerializeField]
	private Constraint[] constraints;

	//Sprite image
	public Texture2D texture;
	[SerializeField]
	private Texture2D _texture;

	//Tint color
	public Color color=Color.white;
	[SerializeField]
	private Color _color;

	//Defines which points will be fixed in space
	public enum fixTypes{fixedAll,fixedTwo,fixedThree};
	public fixTypes fixType=fixTypes.fixedTwo;
	[SerializeField]
	private fixTypes? _fixType=null;

	//Number of points both vertical and horizontal, keping it the same
	//[Range(2,11)]
	[Range(2,69)]
	public int resolution=11; //Defaulting to an odd number because it looks better when there's three fixed points (fixTypes.fixedThree)
	[SerializeField]
	private int _resolution;
	[SerializeField]
	private int pointsX=10;
	[SerializeField]
	private int pointsY=10;

	//Sets if diagonal connections would be used
	public enum constraintTypes{Neighbor,Diagonal};
	public constraintTypes constraintType=constraintTypes.Neighbor;
	[SerializeField]
	private constraintTypes? _constraintType=null;

	//Sets how many computation passes will be performed
	[Range(1,5)]
	public int passes=2;
	[SerializeField]
	private int _passes;

	//Sets mass of each point
	[Range(0.01f,1f)]
	public float mass=0.25f;
	[SerializeField]
	private float _mass;

	//Sets stiffness of vertical and horizontal constraints
	[Range(0.01f,1f)]
	public float stiffnessNeighbor=0.9f;
	[SerializeField]
	private float _stiffnessNeighbor;

	//Sets stiffness of diagonal constraints
	[Range(0.01f,1f)]
	public float stiffnessDiagonal=0.1f;
	[SerializeField]
	private float _stiffnessDiagonal;

	//How much do we multiply the collision force
	[Range(0.01f,2f)]
	public float forceMultiplier=1f;
	[SerializeField]
	private float _forceMultiplier;

	//Wind settings
	public bool wind=false;
	[SerializeField]
	private bool _wind;

	[Range(0f,360f)]
	public float windDirection=0f;
	[SerializeField]
	private float _windDirection;

	public float windForce=0.75f;
	[SerializeField]
	public float _windForce;

	[Range(0f,1f)]
	public float windChange=1f;
	[SerializeField]
	public float _windChange;

	public float windChangeSpeed=2f;
	[SerializeField]
	public float _windChangeSpeed;

	//A wind vector calculated from settings above
	[SerializeField]
	private Vector2 windVector;
	private Vector2 windVectorDir;
	public Vector2 windVectorForce;

	//For calculating wind
	[SerializeField]
	private float radians;
	[SerializeField]
	private float windForceFinal;

	//These are calculated based on provided image
	[SerializeField]
	private float meshWidth=1f;
	[SerializeField]
	private float meshHeight=1f;
	[SerializeField]
	private float meshDiagonal;

	//Material to use
	public enum materialTypes{Unlit,Lit};
	public materialTypes materialType=materialTypes.Unlit;
	[SerializeField]
	private materialTypes? _materialType=null;

	//Sprite sorting properties
	public int sortingLayer=0;
	[SerializeField]
	private int _sortingLayer;
	public int orderInLayer=0;
	[SerializeField]
	private int _orderInLayer=0;

	//Size of one piece of cloth - one square limited by any 4 points in rest position
	[SerializeField]
	private Vector2 pieceDimensions;
	[SerializeField]
	private float pieceDiagonal;

	//To calculate sleep timer
	float biggestMoveSQR=0f;
	float sleepTimer=0f;
	public bool sleep=false;

	//Last transform position before change
	Vector3 lastPosition;
	//Change in position
	Vector3 positionDelta=Vector3.zero;

	//Forces to recreate constraints once
	private bool recreateConstraints=false;

	[SerializeField]
	private bool? isSRP;
	[SerializeField]
	private bool? _isSRP;

	private string TextureProperty {
		get {
			if(UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset == null || materialType==materialTypes.Unlit)
			{
				return "_MainTex";
			}
			else
			{ 
				return "_BaseMap";
			}
		}
	}

	private string ColorProperty {
		get {
			if(UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset == null || materialType==materialTypes.Unlit)
			{
				return "_Color";
			}
			else
			{ 
				return "_BaseColor";
			}
		}
	}

	#endregion

	#region Context menu

	[ContextMenu("ClothSprite online manual...")]
	void OpenURLOnlineManual(){
		Application.OpenURL("http://ax23w4.com/devlog/clothsprite");
	}

	[ContextMenu("Rate this asset on the Asset Store...")]
	void OpenURLRateThisAsset(){
		Application.OpenURL("http://u3d.as/1Wc3");
	}

	[ContextMenu("Other assets by Andrii Sudyn...")]
	void OpenURLOtherAssets(){
		Application.OpenURL("https://assetstore.unity.com/publishers/26071");
	}

	#endregion

	#region Bootstrap

	private void Awake(){
		mr=GetComponent<MeshRenderer>();
		mf=GetComponent<MeshFilter>();
		col=GetComponent<PolygonCollider2D>();
		mf.sharedMesh=null; //Reset shared mesh and material 
		mr.sharedMaterial=null; //in case the object was duplicated
		col.isTrigger=true; //Set collider to trigger
		lastPosition=transform.position;
		isSRP=UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null;
		SetMeshAndMaterial();
		Update();
	}

	void OnDrawGizmos(){
		#if UNITY_EDITOR
		if(!Application.isPlaying && !sleep){
			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			UnityEditor.SceneView.RepaintAll();
		}
		#endif
	}

	private void OnBecameInvisible(){
		col.enabled=false;
		sleep=true;
	}

	private void OnBecameVisible(){
		col.enabled=true;
		WakeUp();
	}

	#endregion

	#region Calculate point positions each frame

	void Update(){
		#if UNITY_EDITOR
		if(!sleep) UnityEditor.EditorApplication.delayCall+=UnityEditor.EditorApplication.QueuePlayerLoopUpdate;
		#endif
		//First we check if any attribute that relates to creation of the points changed or if points were created at all
		if(_resolution!=resolution || _fixType!=fixType || _texture!=texture || points==null){
			//If size of the grid changes we need to destroy the mesh so it will be recreated
			if(_resolution!=resolution) mesh.Clear();
			pointsX=resolution;
			pointsY=resolution;
			_resolution=resolution;
			_fixType=fixType;
			if(texture!=null){
				meshWidth=(float)texture.width/100f;
				meshHeight=(float)texture.height/100f;
			}else{
				meshWidth=1f;
				meshHeight=1f;
			}
			meshDiagonal=Mathf.Sqrt((meshWidth*meshWidth)+(meshHeight*meshHeight));
			points=new Point[pointsX,pointsY];
			for(int x=0;x<this.pointsX;x++){
				for(int y=0;y<this.pointsY;y++){
					//Decide position of the point
					Vector3 position=new Vector3((float)x*(meshWidth/(pointsX-1))-meshWidth/2,(float)y*(-meshHeight/(pointsY-1)),0f);
					//Decide if this point should be fixed in space, immovable by gravity and constraints
					int isFixed=0;
					if(fixType==fixTypes.fixedAll && y==0) isFixed=1;
					else if(fixType==fixTypes.fixedTwo && (y==0 && (x==0 || x==pointsX-1))) isFixed=1;
					else if(fixType==fixTypes.fixedThree && (y==0 && (x==0 || x==pointsX-1 || x==Mathf.Floor((pointsX-1)/2f) || x==Mathf.Ceil((pointsX-1)/2f)))) isFixed=1;
					//Create a point of the simulated cloth
					points[x,y]=new Point{position=position,prevPosition=position,acceleration=Vector3.zero,isFixed=isFixed};
				}
			}
			pieceDimensions=new Vector2(meshWidth/(pointsX-1),meshHeight/(pointsY-1));
			pieceDiagonal=Mathf.Sqrt((pieceDimensions.x*pieceDimensions.x)+(pieceDimensions.y*pieceDimensions.y));
			recreateConstraints=true;
			WakeUp();
		}
		//If constraints need to be recreated
		if(_constraintType!=constraintType || recreateConstraints){ 
			//Calculate the number of neighbor constraints
			int cLenght=((pointsX-1)*pointsY)+((pointsY-1)*pointsX);
			//Add number of diagonal constraints
			if(constraintType==constraintTypes.Diagonal) cLenght+=(pointsX-1)*(pointsY-1)*2;
			//Create constraint array of required length
			if(constraints==null || constraints.Length!=cLenght) constraints=new Constraint[cLenght];
			//Fill constraints array with all the constraints
			int cc=0;
			for(int x=0;x<pointsX;x++){
				for(int y=0;y<pointsY;y++){
					//Point to the right
					if(x<pointsY-1) constraints[cc++]=new Constraint(){x1=x,y1=y,x2=x+1,y2=y};
					//Point to the bottom
					if(y<pointsY-1) constraints[cc++]=new Constraint(){x1=x,y1=y,x2=x,y2=y+1};
					//Points on the diagonal
					if(constraintType==constraintTypes.Diagonal){
						if(y<pointsY-1 && x<pointsX-1) constraints[cc++]=new Constraint(){x1=x,y1=y,x2=x+1,y2=y+1};
						if(y<pointsY-1 && x>0) constraints[cc++]=new Constraint(){x1=x,y1=y,x2=x-1,y2=y+1};
					}
				}
			}
			_constraintType=constraintType;
			recreateConstraints=false;
			WakeUp();
		}
		//If wind changes
		if(_wind!=wind || _windDirection!=windDirection || _windForce!=windForce || _windChange!=windChange || _windChangeSpeed!=windChangeSpeed){
			radians=windDirection*(Mathf.PI/180);
			windVector=new Vector3(Mathf.Cos(radians),Mathf.Sin(radians),0);
			windForceFinal=windForce*0.01f;
			_wind=wind;
			_windDirection=windDirection;
			_windForce=windForce;
			_windChange=windChange;
			_windChangeSpeed=windChangeSpeed;
			WakeUp();
		}
		
		//If transform has moved, we get the position delta
		if(transform.hasChanged){ 
			positionDelta=transform.position-lastPosition;
			lastPosition=transform.position;
			transform.hasChanged=false;
			sleep=false;
			for(int x=0;x<pointsX;x++){
				for(int y=0;y<pointsY;y++){
					if(points[x,y].isFixed==0){
						points[x,y].position-=positionDelta;
						points[x,y].prevPosition-=positionDelta;
					}
				}
			}
			positionDelta=Vector3.zero;
		}

		//Deal with actual simulation
		if(!sleep){
			if(wind){
				windVectorDir=RotateV2(windVector,(Mathf.PerlinNoise(Time.time*windChangeSpeed,0)-0.5f)*windChange*90f);
			}
			//Calculate each point's movement due to previous position and gravity
			Vector3 gravity=transform.InverseTransformDirection((Physics2D.gravity*mass)*(Time.timeScale/1000f));
			for(int x=0;x<pointsX;x++){
				for(int y=0;y<pointsY;y++){
					if(points[x,y].isFixed==0){
						Vector3 temp=points[x,y].position;
						points[x,y].position+=((points[x,y].position-points[x,y].prevPosition)*0.95f)+(points[x,y].acceleration*forceMultiplier)+gravity;
						if(wind){
							float turbulance=Mathf.PerlinNoise(Time.time*windChangeSpeed,points[x,y].position.y*windChangeSpeed)*windChange*2f-windChange;
							windVectorForce=windVectorDir*(windForceFinal+windForceFinal*turbulance);
							points[x,y].position+=transform.InverseTransformDirection(windVectorForce);
						}
						points[x,y].prevPosition=temp;
						points[x,y].acceleration=Vector3.zero;
					}
				}
			}
			//Solve constraints
			SolveConstraints();
			//Update collider points
			int midPoint=pointsX/2;
			colPoints[0]=points[0,0].position;
			colPoints[1]=points[midPoint,1].position;
			colPoints[2]=points[pointsX-1,0].position;
			colPoints[3]=points[pointsX-1,midPoint].position;
			colPoints[4]=points[pointsX-1,pointsY-1].position;
			colPoints[5]=points[midPoint,pointsY-1].position;
			colPoints[6]=points[0,pointsY-1].position;
			colPoints[7]=points[0,midPoint].position;
			col.points=colPoints;
			//Find biggest distance any point has moved
			biggestMoveSQR=0f;
			for(int x=0;x<pointsX;x++){
				for(int y=0;y<pointsY;y++){
					if(points[x,y].isFixed==0){
						biggestMoveSQR=Mathf.Max((points[x,y].position-points[x,y].prevPosition).sqrMagnitude,biggestMoveSQR);
					}
				}
			}
			//Remember last time when this object had a point moved by a significant distance
			if(biggestMoveSQR>0.000001f) sleepTimer=Time.time;
			//If the points didn't move much for 3 seconds we start sleeping
			if(sleepTimer<Time.time-3f) sleep=true;
		}
		//Deal with updates related to the material and mesh renderer
		if(_materialType!=materialType || _texture!=texture || _color!=color || _sortingLayer!=sortingLayer || _orderInLayer!=orderInLayer || _isSRP!=isSRP){
			if(_materialType!=materialType || _isSRP!=isSRP){
				#if UNITY_EDITOR
				DestroyImmediate(mat);
				#else
				Destroy(mat);
				#endif
				_isSRP=isSRP;
			}
			SetMeshAndMaterial(); //Updates mesh or material only if they're null
			WakeUp();
		}
		//Wake up if any of these parameters changes
		if(_passes!=passes || _mass!=mass || _stiffnessNeighbor!=stiffnessNeighbor || _stiffnessDiagonal!=stiffnessDiagonal || _forceMultiplier!=forceMultiplier){ 
			_passes=passes;
			_mass=mass;
			_stiffnessNeighbor=stiffnessNeighbor;
			_stiffnessDiagonal=stiffnessDiagonal;
			_forceMultiplier=forceMultiplier;
			WakeUp();
		}
		if(!sleep){
			if(mesh.vertexCount>0){
				UpdateVertices();
			}else{
				GenerateMesh();
			}
		}
	}

	private void SolveConstraints(){
		//Solve constraints
		for(int pass=0;pass<passes;pass++){
			for(int cc=0;cc<constraints.Length;cc++){
				SolveConstraint(ref constraints[cc]);
			}
		}
	}

	private void ShuffleArray<T>(ref T[] array){ 
		for(int n=array.Length-1;n>1;n--){ 
			int k=Random.Range(0,n);
			T temp=array[n];
			array[n]=array[k];
			array[k]=temp;
		}	
	}

	private void SolveConstraint(ref Constraint c){
		ref Point p1=ref points[c.x1,c.y1];
		ref Point p2=ref points[c.x2,c.y2];
		if(p1.isFixed==0 || p2.isFixed==0){
			//Get rest distance and stiffness based on point positions in the grid
			float restDist=((c.x1-c.x2!=0 && c.y1-c.y2!=0)?pieceDiagonal:(c.x1-c.x2!=0?pieceDimensions.x:pieceDimensions.y));
			float stiffness=((c.x1-c.x2!=0 && c.y1-c.y2!=0)?stiffnessDiagonal:stiffnessNeighbor);
			//Calculate movement
			Vector3 dir=p2.position-p1.position;
			float dist=dir.magnitude;
			float tension=(restDist-dist)/dist;
			Vector3 move=dir*tension*0.5f*stiffness;
			//Move points
			if(p1.isFixed==1){ 
				p2.position+=move+move;
			}else if(p2.isFixed==1){ 
				p1.position-=move+move;
			}else{ 
				p1.position-=move;
				p2.position+=move;
			}
		}
	}

	private void SolveConstraint(ref Point p1,ref Point p2,ref float restDist,ref float stiffnesss){
		if(p1.isFixed==0 || p2.isFixed==0){
			Vector3 dir=p2.position-p1.position;
			float dist=dir.magnitude;
			float tension=(restDist-dist)/dist;
			Vector3 move=dir*tension*0.5f*stiffnesss;
			if(p1.isFixed==1){ 
				p2.position+=move+move;
			}else if(p2.isFixed==1){ 
				p1.position-=move+move;
			}else{ 
				p1.position-=move;
				p2.position+=move;
			}
		}
	}

	public void WakeUp(){
		sleepTimer=Time.time;
		sleep=false;
	}

	#endregion

	#region Detect impacts with Collider2D

	private void OnTriggerEnter2D(Collider2D other){
		Vector2 position=new Vector2();
		#if UNITY_2019_1_OR_NEWER
			position=col.ClosestPoint(other.transform.position);
		#else
			position=col.bounds.ClosestPoint(other.transform.position);
		#endif
		Impact(position,(Vector2)other.bounds.extents,other.GetComponent<Rigidbody2D>().velocity);
	}
	
	void Impact(Vector2 position,Vector2 size,Vector2 force){
		WakeUp();
		force*=0.01f;
		size*=2f; //Just to capture more points
		//Transform input from global space in case the object is scaled or rotated
		position=transform.InverseTransformPoint(position);
		force=transform.InverseTransformVector(force);
		//Don't let the size be smaller that distance between points so we touch at least one point
		if(Mathf.Max(size.x,size.y)<Mathf.Min(pieceDimensions.x,pieceDimensions.y)*2f) size=size.normalized*Mathf.Max(pieceDimensions.x,pieceDimensions.y)*2;
		//Find all points that lay inside the priveded size and apply the force to them
		for(int x=0;x<pointsX;x++){
			for(int y=0;y<pointsY;y++){
				if(points[x,y].isFixed==0){
					if(
						points[x,y].position.x>position.x-size.x && points[x,y].position.x<position.x+size.x &&
						points[x,y].position.y>position.y-size.y && points[x,y].position.y<position.y+size.y
					){ 
						points[x,y].acceleration+=new Vector3(force.x,force.y,0.01f);
					}
				}
			}
		}
	}

	#endregion

	#region Deal with mesh and mesh renderer

	//Create mesh, create a copy of material, set its properties
	public void SetMeshAndMaterial(){
		if(mesh==null){
			mesh=new Mesh();
			mesh.name="ClothSpriteMesh";
			if(mf.sharedMesh!=null) DestroyImmediate(mf.sharedMesh);
		}
		if(mf.sharedMesh==null){
			mf.sharedMesh=mesh;
		}
		if(mat==null){
			if(materialType == materialTypes.Unlit)
			{
				mat = new Material((Material)Resources.Load("ClothSpriteUnlit",typeof(Material)));
			}
			else
			{
				if(UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset == null)
				{ 
					mat = new Material((Material)Resources.Load("ClothSpriteLit",typeof(Material)));
				}
                else
				{ 
					mat = new Material((Material)Resources.Load("ClothSpriteLitURP",typeof(Material)));
				}
			}
		}
		mat.SetTexture(TextureProperty,texture);
		mat.SetColor(ColorProperty,color);
		if(mr.sharedMaterial==null || mr.sharedMaterial!=mat){
			mr.sharedMaterial=mat;
		}
		if(_sortingLayer!=sortingLayer || _orderInLayer!=orderInLayer){
			mr.sortingLayerID=sortingLayer;
			mr.sortingOrder=orderInLayer;
		}
		_materialType=materialType;
		_texture=texture;
		_color=color;
		_sortingLayer=sortingLayer;
		_orderInLayer=orderInLayer;
	}
	
	//Turn points array into mesh
	void GenerateMesh(){
		int verticeNum=0;
		int squareNum=-1;
		vertices.Clear();
		uvs.Clear();
		colors.Clear();
		normals.Clear();
		triangles=new int[(((pointsX-1)*(pointsY-1))*2)*3];
		for(int y=0;y<pointsY;y++){
			for(int x=0;x<pointsX;x++){
				vertices.Add(points[x,y].position);
				uvs.Add(new Vector3(
					((float)x/(float)(pointsX-1)),
					1f-((float)y/(float)(pointsY-1)),
					0f
				));
				normals.Add(Vector3.back);
				if(x>0 && y>0){
					verticeNum=x+(y*pointsX);
					squareNum++;
					triangles[squareNum*6]=verticeNum-pointsX-1;
					triangles[squareNum*6+1]=verticeNum;
					triangles[squareNum*6+2]=verticeNum-1;
					triangles[squareNum*6+3]=verticeNum;
					triangles[squareNum*6+4]=verticeNum-pointsX-1;
					triangles[squareNum*6+5]=verticeNum-pointsX;
				}
			}
		}
		mesh.Clear();
		mesh.MarkDynamic();
		mesh.SetVertices(vertices);
		mesh.SetColors(colors);
		mesh.SetUVs(0,uvs);
		mesh.SetNormals(normals);
		mesh.RecalculateBounds();
		mesh.SetTriangles(triangles,0);
		trianglesCount=triangles.Length/3;
	}

	//Only update vertices of the mesh
	void UpdateVertices(){ 
		int i=0;
		for(int y=0;y<pointsY;y++){
			for(int x=0;x<pointsX;x++){
				vertices[i]=points[x,y].position;
				i++;
			}
		}
		mesh.MarkDynamic();
		mesh.SetVertices(vertices);
		mesh.RecalculateBounds();
	}

	public int triangleCount{
		get{return trianglesCount;}
	}

     private Vector2 RotateV2(Vector2 v,float degrees) {
         float sin=Mathf.Sin(degrees*Mathf.Deg2Rad);
         float cos=Mathf.Cos(degrees*Mathf.Deg2Rad);
         float tx=v.x;
         float ty=v.y;
         v.x=(cos*tx)-(sin*ty);
         v.y=(sin*tx)+(cos*ty);
         return v;
     }

	#endregion

}
