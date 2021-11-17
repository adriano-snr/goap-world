using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Literals {
    public const string builtHousesCount = "BuiltHousesCount";

    public const string currentBuildObjectTag = "currentBuildObjectTag";
    public const string currentBuildStructureTag = "currentBuildStructureTag";

    public const string currentBuilLocation = "currentBuilLocation";
    public const string currentBuildObject = "currentHouseProject";
    public const string currentBuildStructure = "currentHouseStructureProject";

    public const string clearBuildActionV2Info = "clearBuildActionV2Info";

    public const string myHouseLocation = "myHouseLocation";
    public const string myHouseGameObject = "myHouseGameObject";
    public const string myHouseStructure = "myHouseStructure";

    public const string myHouseBuildProgress = "myHouseBuildProgress";
    public const string myHouseBuildStarted = "myHouseBuildStarted";
    public const string myHouseHasResource = "myHouseHasResource";

    public const string buildStarted = "buildStarted";
    public const string buildProgress = "buildProgress";
    public const string buildPosition = "buildPosition";
    public const string buildHasResource = "buildHasResource";
    public const string buildFinished = "buildFinished";


    public const string reconcilePosition = "reconcilePosition";
    public const string isAtPosition = "isAtPosition"; 

    public const string resourceNameWater = "Water";
    public const string resourceNameTree = "Tree";
    public const string resourceNameOre = "Ore";
    public const string resourceNameAxe = "Axe";

    public const string GoalCollectWater = "Goal: Collect Water";
    public const string GoalCollectTree = "Goal: Collect Tree"; 
    public const string GoalCollectOre = "Goal: Collect Ore";
    public const string GoalCollectAxe = "Goal: Collect Axe";
    public const string GoalIddle = "Goal: Iddle";
    public const string GoalBuildHouse = "Goal: Build House";
    public const string GoalBuildBank = "Goal: Build Bank";
    public const string GoalStockpileResource = "Goal: Stockpile Resource";

    public const string ActionIddle = "Action: Iddle";
    public const string ActionGatherResource = "Action: Gather Resource";

    public const string hasEaten = "hasEaten";


    public const string impossibleGoal = "[IMPOSSIBLE]";
    public const string hasResource = "hasResource";
    public const string collectedResource = "collectedResource";
    private const string collectedQuantity = "collectedQuantity";
    private const string hasQuantity = "hasQuantity";
    public static string CollectedResource(string resourceName) => collectedResource + resourceName;
    public static string CollectedQuantityResource(string resourceName) => collectedQuantity + resourceName;
    public static string HasQuantityResource(string resourceName) => hasQuantity + resourceName;
    public static string HasResource(string resourceName) => hasResource + resourceName;
    public static string BuildHasResource(string resourceName) => buildHasResource + resourceName;
    public static string BuildFinished(string blueprintName) => buildFinished + blueprintName;
    public static bool IsCollectGoal(string goalName) => goalName.StartsWith("Goal: Collect");
    public static string MyHouseHasResource(string resourceName) => myHouseHasResource + resourceName;
}
