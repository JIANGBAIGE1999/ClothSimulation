using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
using System;

[CustomEditor(typeof(ClothSprite))]
public class ClothSpriteEditor:Editor{

	ClothSprite script;

	[MenuItem("GameObject/2D Object/ClothSprite")]
	static void Create(){
		GameObject go=new GameObject();
		go.AddComponent<ClothSprite>();
		go.name="ClothSprite";
		SceneView sc=SceneView.lastActiveSceneView!=null?SceneView.lastActiveSceneView:SceneView.sceneViews[0] as SceneView;
		go.transform.position=new Vector3(sc.pivot.x,sc.pivot.y+0.5f,0f);
		if(Selection.activeGameObject!=null) go.transform.parent=Selection.activeGameObject.transform;
		Selection.activeGameObject=go;
		ClothSprite tScript=go.GetComponent<ClothSprite>();
		UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(tScript.GetComponent<MeshFilter>(),false);
		UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(tScript.GetComponent<MeshRenderer>(),false);
		UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(tScript.GetComponent<PolygonCollider2D>(),false);
	}

	void Awake(){
		script=(ClothSprite)target;
	}

	void OnSceneGUI(){
		script=(ClothSprite)target;
		Event e=Event.current;
		float size=HandleUtility.GetHandleSize(Vector3.zero)*0.02f;


		if(script.wind){
			Vector3 shadowDir=new Vector3(-size,size,0f);
			Vector3 arrowCenter=script.transform.position+Vector3.up;
			Vector3 halfArrow=(Vector3)script.windVectorForce*100f*0.5f;
			Vector3 halfArrowUp=Vector2.Perpendicular(halfArrow).normalized*0.1f;

			Handles.color=new Color(1f,1f,1f,1f);
			Handles.DrawLine(arrowCenter-halfArrow+shadowDir,arrowCenter+halfArrow+shadowDir);
			Handles.DrawLine(arrowCenter+halfArrow+halfArrowUp-halfArrow.normalized*0.1f+shadowDir,arrowCenter+halfArrow+shadowDir);
			Handles.DrawLine(arrowCenter+halfArrow-halfArrowUp-halfArrow.normalized*0.1f+shadowDir,arrowCenter+halfArrow+shadowDir);

			Handles.color=new Color(0f,0f,0f,0.5f);
			Handles.DrawLine(arrowCenter-halfArrow,arrowCenter+halfArrow);
			Handles.DrawLine(arrowCenter+halfArrow+halfArrowUp-halfArrow.normalized*0.1f,arrowCenter+halfArrow);
			Handles.DrawLine(arrowCenter+halfArrow-halfArrowUp-halfArrow.normalized*0.1f,arrowCenter+halfArrow);
		}

		/*
		//Draw simulated points in scene
		if(script.points!=null){
			Handles.color=new Color(1f,1f,1f,0.5f);
			for(int x=0;x<script.points.GetLength(0);x++){
				for(int y=0;y<script.points.GetLength(1);y++){
					Handles.DrawSolidDisc(script.transform.TransformPoint(script.points[x,y].position),Vector3.back,size);
				}
			}
		}
		*/
	}




	public override void OnInspectorGUI(){

		//Texture field
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(new GUIContent("Sprite","An image to use as a texture"));
		Texture2D texture=(Texture2D)EditorGUILayout.ObjectField(script.texture,typeof(Texture2D),false);
		if(script.texture!=texture){
			Undo.RecordObject(script,"Change sprite");
			script.texture=texture;
			EditorUtility.SetDirty(script);
		}
		EditorGUILayout.EndHorizontal();

		//Tint field
		Color tint=EditorGUILayout.ColorField(new GUIContent("Color","Tint the sprite using a color. Use white for no tint."),script.color);
		if(script.color!=tint){
			Undo.RecordObject(script,"Change color");
			script.color=tint;
			EditorUtility.SetDirty(script);
		}

		//Switch for fix type
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(new GUIContent("Fixed points","Which points on top of the sprite will be fixed"));
		string[] enumstringsFixType=ClothSprite.fixTypes.GetNames(typeof(ClothSprite.fixTypes));
		Texture[] buttonsFixType=new Texture[enumstringsFixType.Length];
		for(int i=0;i<buttonsFixType.Length;i++){
			buttonsFixType[i]=(Texture)Resources.Load("Icons/"+enumstringsFixType[i]);
		}
		int switchStateFixType=GUILayout.Toolbar((int)script.fixType,buttonsFixType,GUILayout.Height(20));
		if(switchStateFixType!=(int)script.fixType){
			Undo.RecordObject(script,"Change fix type");
			GUI.FocusControl(null);
			script.fixType=(ClothSprite.fixTypes)Enum.Parse(typeof(ClothSprite.fixTypes),enumstringsFixType[switchStateFixType]);
			EditorUtility.SetDirty(script);
		}
		EditorGUILayout.EndHorizontal();

		//Edit resolution
		//int resolution=EditorGUILayout.IntSlider(new GUIContent("Resolution","How many simulated points to create vertically and horizontally"),script.resolution,2,21);
		int resolution=EditorGUILayout.IntSlider(new GUIContent("Resolution","How many simulated points to create vertically and horizontally"),script.resolution,2,51);
		if(resolution!=script.resolution){
			Undo.RecordObject(script,"Change resolution");
			script.resolution=resolution;
			EditorUtility.SetDirty(script);
		}

		//Show triangle count
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(EditorGUIUtility.labelWidth);
		GUILayout.Box(new GUIContent("Triangle count: "+script.triangleCount.ToString()),EditorStyles.helpBox);
		EditorGUILayout.EndHorizontal();

		//Switch for fix type
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(new GUIContent("Point connections","Each point is connected to its neighbors. Adding diagonal connections allows to spread the cloth better but doubles the calculations."));
		string[] enumstringsConstraintType=ClothSprite.constraintTypes.GetNames(typeof(ClothSprite.constraintTypes));
		Texture[] buttonsConstraintType=new Texture[enumstringsConstraintType.Length];
		for(int i=0;i<buttonsConstraintType.Length;i++){
			buttonsConstraintType[i]=(Texture)Resources.Load("Icons/constraints"+enumstringsConstraintType[i]);
		}
		int switchStateConstraintType=GUILayout.Toolbar((int)script.constraintType,buttonsConstraintType,GUILayout.Height(20));
		if(switchStateConstraintType!=(int)script.constraintType){
			Undo.RecordObject(script,"Change point connections");
			GUI.FocusControl(null);
			script.constraintType=(ClothSprite.constraintTypes)Enum.Parse(typeof(ClothSprite.constraintTypes),enumstringsConstraintType[switchStateConstraintType]);
			EditorUtility.SetDirty(script);
		}
		EditorGUILayout.EndHorizontal();

		//Passes
		int passes=EditorGUILayout.IntSlider(new GUIContent("Computation passes","Number of times the calculations are performed for each connection. Increasing it makes the cloth more stiff."),script.passes,1,5);
		if(passes!=script.passes){
			Undo.RecordObject(script,"Change passes");
			script.passes=passes;
			EditorUtility.SetDirty(script);
		}

		//Mass
		float mass=EditorGUILayout.Slider(new GUIContent("Mass","How much each single simulated point weights"),script.mass,0.01f,1f);
		if(mass!=script.mass){
			Undo.RecordObject(script,"Changed mass");
			script.mass=mass;
			EditorUtility.SetDirty(script);
		}

		//Stiffness
		float stiffnessNeighbor=EditorGUILayout.Slider(new GUIContent("Stiffness","How stiff or relaxed are connections between neighboring points"),script.stiffnessNeighbor,0.01f,1f);
		if(stiffnessNeighbor!=script.stiffnessNeighbor){
			Undo.RecordObject(script,"Changed neighbor stiffness");
			script.stiffnessNeighbor=stiffnessNeighbor;
			EditorUtility.SetDirty(script);
		}

		//Diagonal stiffness
		if(script.constraintType==ClothSprite.constraintTypes.Diagonal){ 
			float stiffnessDiagonal=EditorGUILayout.Slider(new GUIContent("Stiffness (diagonal)","How stiff or relaxed are diagonal connections between points"),script.stiffnessDiagonal,0.01f,1f);
			if(stiffnessDiagonal!=script.stiffnessDiagonal){
				Undo.RecordObject(script,"Changed diagonal stiffness");
				script.stiffnessDiagonal=stiffnessDiagonal;
				EditorUtility.SetDirty(script);
			}
		}

		//Show sleep status
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(EditorGUIUtility.labelWidth);
		GUILayout.Box(new GUIContent("Simulation: "+(!script.sleep?"active":"asleep")),EditorStyles.helpBox);
		EditorGUILayout.EndHorizontal();

		//Impact force
		float forceMultiplier=EditorGUILayout.Slider(new GUIContent("Impact force","How the push from other 2D colliders will be"),script.forceMultiplier,0.01f,1f);
		if(forceMultiplier!=script.forceMultiplier){
			Undo.RecordObject(script,"Changed impact force");
			script.forceMultiplier=forceMultiplier;
			EditorUtility.SetDirty(script);
		}

		//Wind
		bool wind=EditorGUILayout.Toggle(new GUIContent("Wind","Apply a wind to the cloth"),script.wind);
		if(wind!=script.wind){
			Undo.RecordObject(script,"Toggle wind");
			script.wind=wind;
			EditorUtility.SetDirty(script);
		}

		if(wind){
			//Wind direction
			float windDirection=EditorGUILayout.Slider(new GUIContent("Wind direction","Direction of the wind in degrees"),script.windDirection,0f,360f);
			if(windDirection!=script.windDirection){
				Undo.RecordObject(script,"Changed wind direction");
				script.windDirection=windDirection;
				EditorUtility.SetDirty(script);
			}
			//Wind force
			float windForce=EditorGUILayout.Slider(new GUIContent("Wind force","How strong the wind will be"),script.windForce,0.01f,10f);
			if(windForce!=script.windForce){
				Undo.RecordObject(script,"Changed wind force");
				script.windForce=windForce;
				EditorUtility.SetDirty(script);
			}
			//Wind change
			float windChange=EditorGUILayout.Slider(new GUIContent("Wind change","How much wind is changing all the time"),script.windChange,0f,1f);
			if(windChange!=script.windChange){
				Undo.RecordObject(script,"Changed wind chnage");
				script.windChange=windChange;
				EditorUtility.SetDirty(script);
			}
			//Wind change speed
			float windChangeSpeed=EditorGUILayout.Slider(new GUIContent("Wind change speed","How fast wind is changing"),script.windChangeSpeed,0.01f,10f);
			if(windChangeSpeed!=script.windChangeSpeed){
				Undo.RecordObject(script,"Changed wind change speed");
				script.windChangeSpeed=windChangeSpeed;
				EditorUtility.SetDirty(script);
			}
		}

		GUILayout.Space(10);


		//Allow to switch between lit and unlit material
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(new GUIContent("Sprite material","The type of material to use for this object"));
		string[] enumStringsMaterialType=ClothSprite.materialTypes.GetNames(typeof(ClothSprite.materialTypes));
		GUIContent[] buttonsMaterialType=new GUIContent[enumStringsMaterialType.Length];
		for(int i=0;i<buttonsMaterialType.Length;i++){
			buttonsMaterialType[i]=new GUIContent(enumStringsMaterialType[i]);
		}
		int switchState=GUILayout.Toolbar((int)script.materialType,buttonsMaterialType);
		if(switchState!=(int)script.materialType){
			GUI.FocusControl(null);
			script.materialType=(ClothSprite.materialTypes)Enum.Parse(typeof(ClothSprite.materialTypes),enumStringsMaterialType[switchState]);
		}
		EditorGUILayout.EndHorizontal();

		//Get sorting layers
		int[] layerIDs=GetSortingLayerUniqueIDs();
		string[] layerNames=GetSortingLayerNames();
		//Get selected sorting layer
		int selected=-1;
		for(int i=0;i<layerIDs.Length;i++){
			if(layerIDs[i]==script.sortingLayer){
				selected=i;
			}
		}
		//Select Default layer if no other is selected
		if(selected==-1){
			for(int i=0;i<layerIDs.Length;i++){
				if(layerIDs[i]==0){
					selected=i;
				}
			}
		}
		//Sorting layer dropdown
		EditorGUI.BeginChangeCheck();
		selected=EditorGUILayout.Popup("Sorting Layer",selected,layerNames);
		if(EditorGUI.EndChangeCheck()){
			Undo.RecordObject(script,"Change sorting layer");
			script.sortingLayer=layerIDs[selected];
			EditorUtility.SetDirty(script);
		}
		//Order in layer field
		EditorGUI.BeginChangeCheck();
		int order=EditorGUILayout.IntField("Order in Layer",script.orderInLayer);
		if(EditorGUI.EndChangeCheck()){
			Undo.RecordObject(script,"Change order in layer");
			script.orderInLayer=order;
			EditorUtility.SetDirty(script);
		}
	}

	//Get the sorting layer IDs
	public int[] GetSortingLayerUniqueIDs() {
		System.Type internalEditorUtilityType=typeof(InternalEditorUtility);
		PropertyInfo sortingLayerUniqueIDsProperty=internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs",BindingFlags.Static|BindingFlags.NonPublic);
		return (int[])sortingLayerUniqueIDsProperty.GetValue(null,new object[0]);
	}

	//Get the sorting layer names
	public string[] GetSortingLayerNames(){
		System.Type internalEditorUtilityType=typeof(InternalEditorUtility);
		PropertyInfo sortingLayersProperty=internalEditorUtilityType.GetProperty("sortingLayerNames",BindingFlags.Static|BindingFlags.NonPublic);
		return (string[])sortingLayersProperty.GetValue(null,new object[0]);
	}

}
