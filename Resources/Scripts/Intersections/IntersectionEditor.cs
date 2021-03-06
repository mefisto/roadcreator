﻿#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Intersection))]
public class IntersectionEditor : Editor
{

    Intersection intersection;
    Tool lastTool;

    public void OnEnable()
    {
        if (intersection == null)
        {
            intersection = (Intersection)target;
        }

        if (intersection.settings == null)
        {
            intersection.settings = RoadCreatorSettings.GetSerializedSettings();
        }

        lastTool = Tools.current;
        Tools.current = Tool.None;

        intersection.FixConnectionReferences();
        intersection.CreateCurvePoints();
    }

    public void OnDisable()
    {
        if (intersection != null)
        {
            intersection.RemoveCurvePoints();
        }

        Tools.current = lastTool;
    }

    public override void OnInspectorGUI()
    {
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontStyle = FontStyle.Bold;

        if (intersection.roundaboutMode == true)
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.Label("Roundabout", guiStyle);
            intersection.roundaboutRadius = Mathf.Clamp(EditorGUILayout.FloatField("Roundabout Radius", intersection.roundaboutRadius), 2, Mathf.Max(2, intersection.maxRoundaboutRadius - intersection.roundaboutWidth));
            intersection.roundaboutWidth = Mathf.Clamp(EditorGUILayout.FloatField("Roundabout Width", intersection.roundaboutWidth), 1, Mathf.Min(20, intersection.roundaboutRadius));
            intersection.physicMaterial = (PhysicMaterial)EditorGUILayout.ObjectField("Physics Material", intersection.physicMaterial, typeof(PhysicMaterial), false);
            intersection.yOffset = Mathf.Max(0, EditorGUILayout.FloatField("Y Offset", intersection.yOffset));

            intersection.stretchTexture = GUILayout.Toggle(intersection.stretchTexture, "Stretch Connection Textures");

            if (intersection.stretchTexture == false)
            {
                intersection.textureScale = Mathf.Clamp(EditorGUILayout.FloatField("Texture Scale", intersection.textureScale), 0.1f, 5);
            }

            intersection.textureTilingY = Mathf.Clamp(EditorGUILayout.FloatField("Texture Tiling Y Multiplier", intersection.textureTilingY), 0.01f, 10);
            intersection.resolutionMultiplier = Mathf.Clamp(EditorGUILayout.FloatField("Resoltion Multiplier", intersection.resolutionMultiplier), 0.01f, 10f);
            intersection.resetCurvePointsOnUpdate = EditorGUILayout.Toggle("Reset Curve Points On Update", intersection.resetCurvePointsOnUpdate);

            GUILayout.Space(20);
            GUILayout.Label("Materials", guiStyle);

            intersection.baseMaterial = (Material)EditorGUILayout.ObjectField("Base Material", intersection.baseMaterial, typeof(Material), false);
            intersection.overlayMaterial = (Material)EditorGUILayout.ObjectField("Overlay Material", intersection.overlayMaterial, typeof(Material), false);
            intersection.connectionBaseMaterial = (Material)EditorGUILayout.ObjectField("Connection Base Material", intersection.connectionBaseMaterial, typeof(Material), false);
            intersection.connectionOverlayMaterial = (Material)EditorGUILayout.ObjectField("Connection Overlay Material", intersection.connectionOverlayMaterial, typeof(Material), false);
            intersection.connectionSectionMaterial = (Material)EditorGUILayout.ObjectField("Connection Section Material", intersection.connectionSectionMaterial, typeof(Material), false);
        }
        else
        {
            EditorGUI.BeginChangeCheck();
            intersection.baseMaterial = (Material)EditorGUILayout.ObjectField("Base Material", intersection.baseMaterial, typeof(Material), false);
            intersection.overlayMaterial = (Material)EditorGUILayout.ObjectField("Overlay Material", intersection.overlayMaterial, typeof(Material), false);
            intersection.physicMaterial = (PhysicMaterial)EditorGUILayout.ObjectField("Physics Material", intersection.physicMaterial, typeof(PhysicMaterial), false);
            intersection.yOffset = Mathf.Max(0.01f, EditorGUILayout.FloatField("Y Offset", intersection.yOffset));
            intersection.stretchTexture = GUILayout.Toggle(intersection.stretchTexture, "Stretch Texture");

            if (intersection.stretchTexture == false)
            {
                intersection.textureScale = Mathf.Clamp(EditorGUILayout.FloatField("Texture Scale", intersection.textureScale), 0.1f, 5);
            }

            intersection.resolutionMultiplier = Mathf.Clamp(EditorGUILayout.FloatField("Resoltion Multiplier", intersection.resolutionMultiplier), 0.01f, 10f);
            intersection.resetCurvePointsOnUpdate = EditorGUILayout.Toggle("Reset Curve Points On Update", intersection.resetCurvePointsOnUpdate);
        }

        if (EditorGUI.EndChangeCheck() == true)
        {
            intersection.CreateMesh();
        }

        if (intersection.roundaboutMode == false && intersection.connections.Count > 2)
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.Space(20);
            GUILayout.Label("Main Roads", guiStyle);

            for (int i = 0; i < intersection.mainRoads.Count; i++)
            {
                intersection.mainRoads[i].open = EditorGUILayout.Foldout(intersection.mainRoads[i].open, "Main Road " + i);

                if (intersection.mainRoads[i].open == true)
                {
                    intersection.mainRoads[i].startIndex = Mathf.Clamp(EditorGUILayout.IntField("Start Index", intersection.mainRoads[i].startIndex), 0, intersection.connections.Count - 1);
                    intersection.mainRoads[i].endIndex = Mathf.Clamp(EditorGUILayout.IntField("End Index", intersection.mainRoads[i].endIndex), 0, intersection.connections.Count - 1);
                    intersection.mainRoads[i].material = (Material)EditorGUILayout.ObjectField("Material", intersection.mainRoads[i].material, typeof(Material), false);
                    intersection.mainRoads[i].flipTexture = EditorGUILayout.Toggle("Flip Texture", intersection.mainRoads[i].flipTexture);

                    if (intersection.mainRoads[i].endIndex == intersection.mainRoads[i].startIndex)
                    {
                        intersection.mainRoads[i].endIndex += 1;

                        if (intersection.mainRoads[i].endIndex >= intersection.connections.Count)
                        {
                            intersection.mainRoads[i].endIndex = 0;
                        }
                    }

                    if (GUILayout.Button("Remove Main Road"))
                    {
                        intersection.mainRoads.RemoveAt(i);
                    }
                }
            }

            if (intersection.mainRoads.Count <= 1 && GUILayout.Button("Add Main Road"))
            {
                intersection.mainRoads.Add(new MainRoad(0, 1, (Material)intersection.settings.FindProperty("defaultIntersectionMainRoadMaterial").objectReferenceValue, false));
            }

            if (EditorGUI.EndChangeCheck() == true)
            {
                intersection.CreateMesh();
            }
        }

        EditorGUI.BeginChangeCheck();

        GUILayout.Space(20);
        GUILayout.Label("Bridge", guiStyle);
        intersection.generateBridge = EditorGUILayout.Toggle("Generate Bridge", intersection.generateBridge);

        if (intersection.generateBridge == true)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bridgeSettings").FindPropertyRelative("bridgeMaterials"), true);
            intersection.bridgeSettings.yOffsetFirstStep = Mathf.Clamp(EditorGUILayout.FloatField("Y Offset First Step", intersection.bridgeSettings.yOffsetFirstStep), 0, 2);
            intersection.bridgeSettings.yOffsetSecondStep = Mathf.Clamp(EditorGUILayout.FloatField("Y Offset Second Step", intersection.bridgeSettings.yOffsetSecondStep), 0, 2);
            intersection.bridgeSettings.widthPercentageFirstStep = Mathf.Clamp(EditorGUILayout.FloatField("Width Percentage First Step", intersection.bridgeSettings.widthPercentageFirstStep), 0, 1);
            intersection.bridgeSettings.widthPercentageSecondStep = Mathf.Clamp(EditorGUILayout.FloatField("Width Percentage Second Step", intersection.bridgeSettings.widthPercentageSecondStep), 0, 1);
            intersection.bridgeSettings.extraWidth = Mathf.Clamp(EditorGUILayout.FloatField("Extra Width", intersection.bridgeSettings.extraWidth), 0, 1);

            GUILayout.Space(20);
            GUILayout.Label("Pillar Placement", guiStyle);

            if (intersection.roundaboutMode == true)
            {
                intersection.placePillars = EditorGUILayout.Toggle("Place Pillars", intersection.placePillars);
            }
            else
            {
                intersection.placePillars = EditorGUILayout.Toggle("Place Pillar", intersection.placePillars);
            }

            if (intersection.placePillars == true)
            {
                intersection.pillarPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", intersection.pillarPrefab, typeof(GameObject), false);

                if (intersection.roundaboutMode == true)
                {
                    intersection.pillarGap = Mathf.Max(0, EditorGUILayout.FloatField("Gap", intersection.pillarGap));
                    intersection.pillarPlacementOffset = Mathf.Max(0, EditorGUILayout.FloatField("Placement Offset", intersection.pillarPlacementOffset));
                    intersection.xPillarScale = Mathf.Max(0.1f, EditorGUILayout.FloatField("X Scale", intersection.xPillarScale));
                    intersection.zPillarScale = Mathf.Max(0.1f, EditorGUILayout.FloatField("Z Scale", intersection.zPillarScale));
                    intersection.pillarRotationDirection = (PrefabLineCreator.RotationDirection)EditorGUILayout.EnumPopup("Rotation Direction", intersection.pillarRotationDirection);
                }
                else
                {
                    intersection.extraPillarHeight = Mathf.Max(0, EditorGUILayout.FloatField("Extra Height", intersection.extraPillarHeight));
                    intersection.xzPillarScale = Mathf.Max(0.1f, EditorGUILayout.FloatField("XZ Scale", intersection.xzPillarScale));
                }
            }
        }

        if (EditorGUI.EndChangeCheck() == true)
        {
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            intersection.CreateMesh();
        }

        GUILayout.Space(20);
        GUILayout.Label("Extra Meshes", guiStyle);
        bool oldOuterExtraMeshesAsRoads = intersection.outerExtraMeshesAsRoads;
        EditorGUI.BeginChangeCheck();

        intersection.outerExtraMeshesAsRoads = EditorGUILayout.Toggle("Outer Extra Meshes Controlled By Roads", intersection.outerExtraMeshesAsRoads);

        if (EditorGUI.EndChangeCheck() == true)
        {
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            intersection.CreateMesh();
        }

        EditorGUI.BeginChangeCheck();
        for (int i = 0; i < intersection.extraMeshes.Count; i++)
        {
            if (intersection.outerExtraMeshesAsRoads == false || (intersection.extraMeshes[i].index == 0 && intersection.roundaboutMode == true))
            {
                intersection.extraMeshes[i].open = EditorGUILayout.Foldout(intersection.extraMeshes[i].open, "Extra Mesh " + i);
                if (intersection.extraMeshes[i].open == true)
                {
                    if (intersection.roundaboutMode == true)
                    {
                        if (intersection.outerExtraMeshesAsRoads == false)
                        {
                            intersection.extraMeshes[i].index = Mathf.Clamp(EditorGUILayout.IntField("Index", intersection.extraMeshes[i].index), 0, intersection.connections.Count * 3);
                        }
                    }
                    else
                    {
                        intersection.extraMeshes[i].index = Mathf.Clamp(EditorGUILayout.IntField("Index", intersection.extraMeshes[i].index), 0, intersection.connections.Count - 1);
                    }

                    intersection.extraMeshes[i].baseMaterial = (Material)EditorGUILayout.ObjectField("Base Material", intersection.extraMeshes[i].baseMaterial, typeof(Material), false);
                    intersection.extraMeshes[i].overlayMaterial = (Material)EditorGUILayout.ObjectField("Overlay Material", intersection.extraMeshes[i].overlayMaterial, typeof(Material), false);
                    intersection.extraMeshes[i].physicMaterial = (PhysicMaterial)EditorGUILayout.ObjectField("Physic Material", intersection.extraMeshes[i].physicMaterial, typeof(PhysicMaterial), false);

                    if (intersection.roundaboutMode == true && intersection.extraMeshes[i].index == 0)
                    {
                        intersection.extraMeshes[i].startWidth = Mathf.Max(EditorGUILayout.FloatField("Width", intersection.extraMeshes[i].startWidth), 0);
                    }
                    else
                    {
                        intersection.extraMeshes[i].startWidth = Mathf.Max(EditorGUILayout.FloatField("Start Width", intersection.extraMeshes[i].startWidth), 0);
                        intersection.extraMeshes[i].endWidth = Mathf.Max(EditorGUILayout.FloatField("End Width", intersection.extraMeshes[i].endWidth), 0);
                    }

                    intersection.extraMeshes[i].flipped = EditorGUILayout.Toggle("Flipped", intersection.extraMeshes[i].flipped);
                    intersection.extraMeshes[i].yOffset = EditorGUILayout.FloatField("Y Offset", intersection.extraMeshes[i].yOffset);

                    if (GUILayout.Button("Duplicate Extra Mesh") == true)
                    {
                        intersection.extraMeshes.Add(intersection.extraMeshes[intersection.extraMeshes.Count - 1].Copy());
                        intersection.CreateExtraMesh();
                    }

                    if (GUILayout.Button("Remove Extra Mesh") == true && intersection.transform.GetChild(0).childCount > 0)
                    {
                        intersection.extraMeshes.RemoveAt(i);

                        for (int j = 0; j < targets.Length; j++)
                        {
                            DestroyImmediate(intersection.transform.GetChild(0).GetChild(i).gameObject);
                        }
                    }
                }
            }
        }

        if (intersection.roundaboutMode == true || intersection.outerExtraMeshesAsRoads == false)
        {
            if (GUILayout.Button("Add Extra Mesh"))
            {
                intersection.extraMeshes.Add(new ExtraMesh(true, 0, (Material)intersection.settings.FindProperty("defaultBaseMaterial").objectReferenceValue, (Material)intersection.settings.FindProperty("defaultExtraMeshOverlayMaterial").objectReferenceValue, null, 1, 1, false, 0));
                intersection.CreateExtraMesh();
            }
        }

        if (EditorGUI.EndChangeCheck() == true)
        {
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            intersection.CreateMesh();
        }

        GUILayout.Space(20);

        if (intersection.resetCurvePointsOnUpdate == false && GUILayout.Button("Reset Curve Points"))
        {
            intersection.ResetCurvePointPositions();
            intersection.CreateCurvePoints();
            intersection.CreateMesh();
        }

        if (intersection.roundaboutMode == true)
        {
            if (GUILayout.Button("Generate Roundabout"))
            {
                intersection.CreateMesh();
            }
        }
        else if (GUILayout.Button("Generate Intersection"))
        {
            intersection.CreateMesh();
        }
    }

    private void OnSceneGUI()
    {
        if (intersection != null)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit raycastHit;

            // Connection Points
            if (Physics.Raycast(ray, out raycastHit, 100, ~(1 << LayerMask.NameToLayer("Intersection") | 1 << LayerMask.NameToLayer("Road"))))
            {
                Vector3 hitPosition = raycastHit.point;
                if (Event.current.control == true)
                {
                    hitPosition = Misc.Round(hitPosition);
                }

                intersection.MovePoints(raycastHit, hitPosition, Event.current, false);
            }

            // Curve points
            if (Physics.Raycast(ray, out raycastHit, 100, ~(1 << LayerMask.NameToLayer("Ignore Mouse Ray"))))
            {
                Vector3 hitPosition = raycastHit.point;
                if (Event.current.control == true)
                {
                    hitPosition = Misc.Round(hitPosition);
                }

                intersection.MovePoints(raycastHit, hitPosition, Event.current, true);
                Draw(raycastHit);
            }

            intersection.transform.parent.GetComponent<RoadSystem>().ShowCreationButtons();
            SceneView.currentDrawingSceneView.Repaint();

            if (intersection.transform.hasChanged == true)
            {
                intersection.CreateMesh();
                intersection.transform.rotation = Quaternion.identity;
                intersection.transform.localScale = Vector3.one;
                intersection.transform.hasChanged = false;
            }
        }
    }

    private void Draw(RaycastHit raycastHit)
    {
        if (intersection.resetCurvePointsOnUpdate == false)
        {
            Handles.color = intersection.settings.FindProperty("intersectionColour").colorValue;
            for (int i = 1; i < intersection.transform.childCount; i++)
            {
                if (intersection.transform.GetChild(i).name != "Bridge")
                {
                    Misc.DrawPoint((RoadCreatorSettings.PointShape)intersection.settings.FindProperty("pointShape").intValue, intersection.transform.GetChild(i).position, intersection.settings.FindProperty("pointSize").floatValue);
                }
            }
        }

        Handles.color = intersection.settings.FindProperty("pointColour").colorValue;
        for (int i = 0; i < intersection.connections.Count; i++)
        {
            Misc.DrawPoint((RoadCreatorSettings.PointShape)intersection.settings.FindProperty("pointShape").intValue, intersection.connections[i].road.transform.position, intersection.settings.FindProperty("pointSize").floatValue);
        }

        Handles.color = intersection.settings.FindProperty("cursorColour").colorValue;

        if (raycastHit.transform != null && raycastHit.transform.name.Contains("Point"))
        {
            Misc.DrawPoint((RoadCreatorSettings.PointShape)intersection.settings.FindProperty("pointShape").intValue, new Vector3(raycastHit.point.x, raycastHit.transform.position.y, raycastHit.point.z), intersection.settings.FindProperty("pointSize").floatValue, false);
        }
        else
        {
            Misc.DrawPoint((RoadCreatorSettings.PointShape)intersection.settings.FindProperty("pointShape").intValue, raycastHit.point, intersection.settings.FindProperty("pointSize").floatValue, false);
        }
    }

}
#endif