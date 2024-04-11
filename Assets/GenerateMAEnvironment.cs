using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using UnityEngine.TextCore.LowLevel;
using UnityEditor; // Import the standard message type for Int32MultiArray


public class GenerateMAEnvironment : MonoBehaviour
{
    public MAData maData; // Reference to the MAData ScriptableObject
    public GameObject floorContainerPrefab; // Prefab for floor container
    public Vector2 roomSize; // Size of the room
    public int areaSize; // Define the size of each area
    public Vector3 tileSize = new Vector3(0.25f, 0.25f, 0.25f);
    public Vector3 wallSize = new Vector3(1, 1, 1);

    public GameObject wallPrefabA;
    public GameObject wallPrefabB;
    public GameObject wallPrefabC;
    public GameObject agentPrefab;
    public GameObject pillarPrefab;
    public GameObject ballPrefab; 
    public GameObject zonePrefab;
    public GameObject pressurePlatePrefab;

    public GameObject wallToNarrowDoorPrefab;
    public GameObject narrowDoorPrefab;
    public GameObject wideDoorPrefab;
    public GameObject narrowBlockedDoorPrefab;
    public GameObject wideBlockedDoorPrefab;

    // Add a dictionary to hold references to the base_link of each agent
    private Dictionary<GameObject, Transform> agentBaseLinks = new Dictionary<GameObject, Transform>();

    // Containers for different elements.
    private GameObject wallContainer;
    private GameObject objectContainer;
    private GameObject floorContainer;
    public GameObject[] objectPrefabs;

    private List<GameObject> agents = new List<GameObject>(); // Lista para manter os agentes
    // Assuming you have a dictionary to store cells by area
    Dictionary<int, List<Cell>> areaCells = new Dictionary<int, List<Cell>>();

    // Function to load and log the data lists
    public void LoadAndLogDataLists()
    {
        // Check if MAData is assigned
        if (maData == null)
        {
            Debug.LogError("MAData reference is not assigned!");
            return;
        }

        InitializeContainers();

        // Log the area data
        // Debug.Log("Area Data:");
        foreach (AreaData areaData in maData.areaDataList)
        {
            // Debug.Log("Area ID: " + areaData.areaNumber);
            // Debug.Log("Robots Dropdown Value: " + areaData.robotsDropdownValue);
            // Debug.Log("Obstacles Dropdown Value: " + areaData.obstaclesDropdownValue);

            // Generate floor for each area
            GenerateArea(areaData.areaNumber);
        }

        PlaceObjectsInAreas();

        PlaceAgentsInAllAreas();
        // Log the door data
        // Debug.Log("Door Data:");
        // foreach (DoorData doorData in maData.doorDataList)
        // {
        //     Debug.Log("Color: " + doorData.color);
        //     Debug.Log("Type: " + doorData.type);
        //     Debug.Log("Color 2: " + doorData.color2);
        //     Debug.Log("Type 2: " + doorData.type2);
        //     Debug.Log("Area: " + doorData.areaNumber);
        // }

        // // Log the object data
        // Debug.Log("Object Data:");
        // foreach (ObjectData objectData in maData.objectDataList)
        // {
        //     Debug.Log("Color: " + objectData.color);
        //     Debug.Log("Type: " + objectData.type);
        //     Debug.Log("Area: " + objectData.areaNumber);
        // }
    }
    
    // Function to generate floor for an area
    void GenerateArea(int areaNumber)
    {
        // Initialize the list for this area if it doesn't exist
        if (!areaCells.ContainsKey(areaNumber))
        {
            areaCells[areaNumber] = new List<Cell>();
        }

        // Get area data
        AreaData areaData = maData.areaDataList.Find(area => area.areaNumber == areaNumber);

        // Adjust the spacing between tiles and area size
        float spacingX = tileSize.x * 4;
        float spacingY = tileSize.y * 4;

        // Calculate the position of the bottom-left corner of the area
        float areaStartX = -roomSize.x / 2 + (((areaNumber - 1) / 2) / Mathf.CeilToInt(roomSize.x / (spacingX * areaSize))) * spacingX * areaSize;
        float areaStartY = -roomSize.y / 2 + (((areaNumber - 1) / (Mathf.CeilToInt(roomSize.x / (spacingX * areaSize)) * 2)) * spacingY * areaSize) + ((areaNumber - 1) % 2 == 0 ? 0 : spacingY * areaSize);

        // Create new container for the current area
        GameObject floorContainer = new GameObject("FloorContainer_" + areaNumber);
        floorContainer.transform.position = new Vector3(areaStartX, 0, areaStartY);

        // Loop through each tile in the area
        for (int x = 0; x < areaSize; x++)
        {
            for (int y = 0; y < areaSize; y++)
            {
                // Calculate the position of the current tile within the area
                Vector3 position = new Vector3(areaStartX + spacingX / 2 + x * spacingX, 0, areaStartY + spacingY / 2 + y * spacingY);

                // Create and instantiate the floor tile
                GameObject floorTileObj = CreateFloorTile(position);
                floorTileObj.transform.parent = floorContainer.transform;

            }
        }

        // Adjustments for spacing to account for cells being twice the size of one tile
        float cellSpacingX = tileSize.x * 8; // Each cell now spans 2 tiles, including spacing
        float cellSpacingY = tileSize.y * 8; // Same as above for Y-axis

        // Loop adjustments: Increment by 2 to cover 2x2 tile area for each cell
        for (int x = 0; x < areaSize; x += 2)
        {
            for (int y = 0; y < areaSize; y += 2)
            {
                // Calculate the center position of the current 2x2 tile area for the cell
                Vector3 cellCenterPosition = new Vector3(
                    areaStartX + cellSpacingX / 2 + x * tileSize.x * 4,
                    0,
                    areaStartY + cellSpacingY / 2 + y * tileSize.y * 4);

                // Adjust isBorder calculation to consider the larger cell size
                bool isBorder = x == 0 || x >= areaSize - 2 || y == 0 || y >= areaSize - 2;

                // No need to create and instantiate a floor tile here as before, unless needed for visual representation
                // Instead, directly create the cell centered in the 2x2 tile area
                Cell newCell = new Cell(cellCenterPosition, true, isBorder);
                areaCells[areaNumber].Add(newCell);
            }
        }



        CreateFloorCollider();

        // Now place the objects in the area
        PlaceObstaclesInArea(areaNumber, floorContainer);

        // Place pillars at the corners based on whether the area number is odd or even
        Vector3[] cornerOffsets;
        if (areaNumber == 1) {
            // Odd area numbers: Place pillars on the right side
            cornerOffsets = new Vector3[]
            {
                new Vector3(areaSize * spacingX, 0, 0), // Bottom-right corner
                new Vector3(0, 0, 0), // bottom-left corner
                new Vector3(0, 0, areaSize * spacingY), // Top-left corner
                new Vector3(areaSize * spacingX, 0, areaSize * spacingY) // Top-right corner
            };
        } else if (areaNumber % 2 == 0)  {
            // Even area numbers: Place pillars on the top side
            cornerOffsets = new Vector3[]
            {
                new Vector3(0, 0, areaSize * spacingY), // Top-left corner
                new Vector3(areaSize * spacingX, 0, areaSize * spacingY) // Top-right corner
            };
        } else {
            // Odd area numbers: Place pillars on the right side
            cornerOffsets = new Vector3[]
            {
                new Vector3(areaSize * spacingX, 0, 0), // Bottom-right corner
                new Vector3(areaSize * spacingX, 0, areaSize * spacingY) // Top-right corner
            };
        }

        foreach (Vector3 offset in cornerOffsets)
        {
            Vector3 pillarPosition = new Vector3(areaStartX + offset.x, 0, areaStartY + offset.z);
            CreatePillarAtPosition(pillarPosition, floorContainer.transform);
        }


        // Define the corner positions for the walls
        Vector3[] wallCorners = {
            new Vector3(areaStartX, 0, areaStartY), // Bottom-left corner
            new Vector3(areaStartX + areaSize * spacingX, 0, areaStartY), // Bottom-right corner
            new Vector3(areaStartX + areaSize * spacingX, 0, areaStartY + areaSize * spacingY), // Top-right corner
            new Vector3(areaStartX, 0, areaStartY + areaSize * spacingY) // Top-left corner
        };

    // Create walls between corner positions for the area
    for (int i = 0; i < 4; i++) // Assuming wallCorners has 4 elements for a rectangular area
    {
        if ((areaNumber == 1 && (i==0 || i==1 || i==3)) ||
            (areaNumber % 2 == 0 && (i==2 || i==3)) ||
            (areaNumber % 2 == 1 && (i==0 || i==1))) {
            Vector3 startCorner = wallCorners[i];
            Vector3 endCorner = wallCorners[(i + 1) % 4]; // Loop around to the first corner after the last one
            CreateWallSegments(startCorner, endCorner, wallContainer.transform);
        } else if ((areaNumber % 2 == 0 && i==1) || (areaNumber % 2 == 1 && i==2)) {
            Vector3 startCorner = wallCorners[i];
            Vector3 endCorner = wallCorners[(i + 1) % 4]; // Loop around to the first corner after the last one
            if (areaNumber > maData.doorDataList.Count){
                CreateWallSegments(startCorner, endCorner, wallContainer.transform);
            } else{
                CreateWallsWithDoors(startCorner, endCorner, wallContainer.transform, maData.doorDataList[areaNumber-1]);
            }
        }
        
    }

    }
    void CreateWallsWithDoors(Vector3 start, Vector3 end, Transform parent, DoorData doorData)
    {
        
        // Calculate the direction and distance between the start and end points
        Vector3 direction = (end - start).normalized;
        float totalDistance = Vector3.Distance(start, end);

        // Initially, assume placing a single door
        int numberOfDoors = doorData.type2 == "None" ? 1 : 2;
        int door1Position = UnityEngine.Random.Range(0, 3);
        int door2Position = UnityEngine.Random.Range(0, 3);
        while (door1Position == door2Position){
            door2Position = UnityEngine.Random.Range(0, 3);
        }

        // Determine the number of wall segments needed, assuming each segment is 1 unit long
        int wallCount = Mathf.Max(1, Mathf.RoundToInt(totalDistance / wallSize.x));
        float segmentLength = totalDistance / wallCount;

        for (int i = 0; i < wallCount; i++)
        {   
            // Calculate the position for this segment
            Vector3 position = start + direction * (segmentLength * i + segmentLength / 2.0f);
            if (i == door1Position){
                if (doorData.type.Contains("narrow"))
                {
                    GameObject wallSegment = Instantiate(wallToNarrowDoorPrefab, position, Quaternion.LookRotation(direction) * Quaternion.Euler(0, -90, 0), parent);
                    wallSegment.transform.localScale = new Vector3(wallSegment.transform.localScale.x * 0.75f, wallSegment.transform.localScale.y*0.65f, wallSegment.transform.localScale.z);

                    GameObject narrowDoor = Instantiate(narrowDoorPrefab, position, Quaternion.LookRotation(direction) * Quaternion.Euler(0, -90, 0), parent);
                    // Change color of the narrow door
                    ChangeDoorColor(narrowDoor, doorData.color);
                    if (doorData.type.Contains("blocked"))
                    {
                        GameObject block = Instantiate(narrowBlockedDoorPrefab, position + Vector3.up * 0.13f, Quaternion.LookRotation(direction), parent);
                        // Change color of the narrow door
                        ChangeDoorColor(block, doorData.color);
                    
                    }
                } 
                else
                {
                    GameObject wideDoor = Instantiate(wideDoorPrefab, position, Quaternion.LookRotation(direction) * Quaternion.Euler(0, -90, 0), parent);
                    
                    // Change color of the wide door
                    ChangeDoorColor(wideDoor, doorData.color);
                    
                    if (doorData.type.Contains("blocked"))
                    {
                        GameObject block = Instantiate(wideBlockedDoorPrefab, position + Vector3.up * 0.13f, Quaternion.LookRotation(direction), parent);
                        // Change color of the narrow door
                        ChangeDoorColor(block, doorData.color);
                    
                    }
                }
            } else if (i == door2Position && doorData.type2 != "None"){
                if (doorData.type.Contains("narrow"))
                {
                    GameObject wallSegment = Instantiate(wallToNarrowDoorPrefab, position, Quaternion.LookRotation(direction) * Quaternion.Euler(0, -90, 0), parent);
                    wallSegment.transform.localScale = new Vector3(wallSegment.transform.localScale.x * 0.75f, wallSegment.transform.localScale.y*0.65f, wallSegment.transform.localScale.z);    
                    GameObject narrowDoor = Instantiate(narrowDoorPrefab, position, Quaternion.LookRotation(direction) * Quaternion.Euler(0, -90, 0), parent);
                    
                    // Change color of the narrow door
                    ChangeDoorColor(narrowDoor, doorData.color2);
                    if (doorData.type2.Contains("blocked"))
                    {
                        GameObject block = Instantiate(narrowBlockedDoorPrefab, position + Vector3.up * 0.13f, Quaternion.LookRotation(direction), parent);
                        // Change color of the narrow door
                        ChangeDoorColor(block, doorData.color2);
                    
                    }
                } 
                else
                {
                    GameObject wideDoor = Instantiate(wideDoorPrefab, position, Quaternion.LookRotation(direction) * Quaternion.Euler(0, -90, 0), parent);
                    
                    // Change color of the wide door
                    ChangeDoorColor(wideDoor, doorData.color2);
                    
                    if (doorData.type2.Contains("blocked"))
                    {
                        GameObject block = Instantiate(wideBlockedDoorPrefab, position + Vector3.up * 0.13f, Quaternion.LookRotation(direction), parent);
                        // Change color of the narrow door
                        ChangeDoorColor(block, doorData.color2);
                    
                    }
                }
            } else{
                GameObject wallPrefab = ChooseRandomWallPrefab();
                GameObject wallSegment = Instantiate(wallPrefab, position, Quaternion.LookRotation(direction) * Quaternion.Euler(0, -90, 0), parent);
                wallSegment.transform.localScale = new Vector3(wallSegment.transform.localScale.x * 0.75f, wallSegment.transform.localScale.y*0.65f, wallSegment.transform.localScale.z);    
            }
        }
    }

    // Method to create wall segments between two points
    void CreateWallSegments(Vector3 start, Vector3 end, Transform parent)
    {
        // Calculate the direction and distance between the start and end points
        Vector3 direction = (end - start).normalized;
        float totalDistance = Vector3.Distance(start, end);

        // Determine the number of wall segments needed, assuming each segment is 1 unit long
        int wallCount = Mathf.Max(1, Mathf.RoundToInt(totalDistance / wallSize.x));
        float segmentLength = totalDistance / wallCount;

        for (int i = 0; i < wallCount; i++)
        {
            // Calculate the position for this segment
            Vector3 position = start + direction * (segmentLength * i + segmentLength / 2.0f);
            
            // Instantiate a wall segment at the calculated position
            GameObject wallPrefab = ChooseRandomWallPrefab(); // Assuming this returns a GameObject
            // Instantiate a wall segment at the calculated position with corrected rotation
            GameObject wallSegment = Instantiate(wallPrefab, position, Quaternion.LookRotation(direction) * Quaternion.Euler(0, -90, 0), parent);

            // Adjust the wall segment's scale to be 75% of its original size
            // Assuming the length of the wall is aligned along the local x-axis
            wallSegment.transform.localScale = new Vector3(wallSegment.transform.localScale.x * 0.75f, wallSegment.transform.localScale.y*0.65f, wallSegment.transform.localScale.z);
            // Optionally, adjust the wall segment's scale if needed
            // wallSegment.transform.localScale = new Vector3(wallSize.x, wallSize.y, wallSize.z);
        }
        
    }
    void PlaceObstaclesInArea(int areaNumber, GameObject floorContainer)
    {
        // Ensure the area exists in the dictionary and there are object prefabs available
        if (!areaCells.ContainsKey(areaNumber) || objectPrefabs == null || objectPrefabs.Length == 0)
        {
            Debug.LogError("Object prefabs are not set or area does not exist.");
            return;
        }

        // Convert the obstaclesDropdownValue to an integer to know how many objects to place
        AreaData areaData = maData.areaDataList.Find(area => area.areaNumber == areaNumber);
        int numberOfObjects = int.Parse(areaData.obstaclesDropdownValue);

        List<Cell> availableCells = areaCells[areaNumber].Where(cell => cell.IsAvailable && !cell.IsBorder).ToList();

        for (int i = 0; i < numberOfObjects; i++)
        {
            if (availableCells.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, availableCells.Count);
                Cell chosenCell = availableCells[randomIndex];
                availableCells.RemoveAt(randomIndex); // Remove the chosen cell from available cells

                // Choose a random object prefab to instantiate
                GameObject objectPrefab = objectPrefabs[UnityEngine.Random.Range(0, objectPrefabs.Length)];
                GameObject instantiatedObject = Instantiate(objectPrefab, chosenCell.Position, Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0), floorContainer.transform);

                // Apply specific transformations based on the prefab name
                if (instantiatedObject.name.Contains("Cube_Prototype_Large_B"))
                {
                    instantiatedObject.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
                    instantiatedObject.transform.position += new Vector3(0, 0.15f, 0);
                }
                else if (instantiatedObject.name.Contains("Box_C") || instantiatedObject.name.Contains("Box_B"))
                {
                    instantiatedObject.transform.localScale = new Vector3(2f, 2f, 2f);
                    instantiatedObject.transform.position += new Vector3(0, 0.15f, 0);
                }
                else if (instantiatedObject.name.Contains("Cube_Prototype_Small"))
                {
                    instantiatedObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                    instantiatedObject.transform.position += new Vector3(0, 0.15f, 0);
                }
                else if (instantiatedObject.name.Contains("Barrel_C"))
                {
                    instantiatedObject.transform.position += new Vector3(0, 0.6f, 0);
                }
                else if (instantiatedObject.name.Contains("Pallet_Small_Decorated_A"))
                {
                    instantiatedObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                    instantiatedObject.transform.position += new Vector3(0, 0.15f, 0);
                }

                // Mark the cell as not available
                chosenCell.IsAvailable = false;
            }
            else
            {
                Debug.LogWarning("Not enough available cells to place all objects in area " + areaNumber);
                break; // Exit the loop if there are no available cells left
            }
        }
    }

    // Updated method to return GameObject instead of Mesh
    GameObject ChooseRandomWallPrefab() 
    {
        int randomIndex = UnityEngine.Random.Range(0, 3);
        switch (randomIndex)
        {
            case 0: return wallPrefabA;
            case 1: return wallPrefabB;
            case 2: return wallPrefabC;
            default: return wallPrefabA;
        }
    }


//     void PlaceBallsInAreas()
//     {
//         foreach (var objectData in maData.objectDataList)
//         {
//             // Ensure the object type is a ball and there's a corresponding area
//             if (objectData.type.Equals("Ball", StringComparison.OrdinalIgnoreCase) && areaCells.ContainsKey(objectData.areaNumber))
//             {
//                 PlaceBallInArea(objectData);
//             }
//         }
//     }

// void PlaceBallInArea(ObjectData objectData)
// {
//     List<Cell> availableCells = areaCells[objectData.areaNumber].Where(cell => cell.IsAvailable && !cell.IsBorder).ToList();
//     if (availableCells.Count > 0)
//     {
//         int randomIndex = UnityEngine.Random.Range(0, availableCells.Count);
//         Cell chosenCell = availableCells[randomIndex];
//         // Assuming you want to raise the ball by 0.5 units above the ground
//         float yOffset = 0.5f;
//         Vector3 adjustedPosition = new Vector3(chosenCell.Position.x, chosenCell.Position.y + yOffset, chosenCell.Position.z);
//         GameObject ball = Instantiate(ballPrefab, adjustedPosition, Quaternion.identity, objectContainer.transform);
        
//         Color ballColor = GetColorFromString(objectData.color); // Use the GetColorFromString method
//         ball.GetComponent<Renderer>().material.color = ballColor;
        
//         chosenCell.IsAvailable = false; // Mark the cell as occupied
//     }
//     else
//     {
//         Debug.LogWarning("No available cells to place ball in area " + objectData.areaNumber);
//     }
// }

    void PlaceObjectsInAreas()
    {
        foreach (var objectData in maData.objectDataList)
        {
            PlaceObjectInArea(objectData);
        }
    }

   void PlaceObjectInArea(ObjectData objectData)
    {
        List<Cell> availableCells = areaCells[objectData.areaNumber].Where(cell => cell.IsAvailable && !cell.IsBorder).ToList();
        if (availableCells.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableCells.Count);
            Cell chosenCell = availableCells[randomIndex];
            
            GameObject objectToInstantiate = null;
            float yOffset = 0; // Default offset for balls

            switch (objectData.type.ToLower())
            {
                case "ball":
                    objectToInstantiate = ballPrefab;
                    yOffset = 0.5f; // Adjust based on the prefab size
                    break;
                case "zone":
                    objectToInstantiate = zonePrefab;
                    yOffset = 0.13f; // Assuming Zone needs to be just above the ground
                    break;
                case "pressure plate":
                    objectToInstantiate = pressurePlatePrefab;
                    yOffset = 0.13f; // Assuming Pressure Plates need to be just above the ground
                    break;
                default:
                    Debug.LogWarning("Unknown type: " + objectData.type);
                    return;
            }

            Vector3 adjustedPosition = new Vector3(chosenCell.Position.x, chosenCell.Position.y + yOffset, chosenCell.Position.z);
            GameObject instantiatedObject = Instantiate(objectToInstantiate, adjustedPosition, Quaternion.identity, objectContainer.transform);
            
            // if (objectData.type.ToLower() == "pressure plate"){
                // Set the color tag
                string colorTag = objectData.color.Replace(" ", ""); // Removes spaces and converts to lower case to match the tag naming convention
                instantiatedObject.tag = colorTag; // Set the object's tag to the color
            // }
            
            // Applying color if the object has a Renderer component
            Color objectColor = GetColorFromString(objectData.color); // Use the previously defined method to convert color name to Color
            Renderer objRenderer = instantiatedObject.GetComponent<Renderer>();
            if (objRenderer != null)
            {
                objRenderer.material.color = objectColor;
            }
            
            chosenCell.IsAvailable = false; // Mark the cell as occupied
        }
        else
        {
            Debug.LogWarning("No available cells to place object in area " + objectData.areaNumber);
        }
    }

    void PlaceAgentsInAllAreas()
    {
        foreach (AreaData area in maData.areaDataList)
        {
            PlaceAgentsInArea(area.areaNumber);
        }
    }

    void PlaceAgentsInArea(int areaNumber)
    {
        if (!areaCells.ContainsKey(areaNumber) || maData.areaDataList == null)
        {
            Debug.LogError("Area data or cells for area " + areaNumber + " not found.");
            return;
        }

        AreaData areaData = maData.areaDataList.Find(area => area.areaNumber == areaNumber);
        if (areaData == null)
        {
            Debug.LogError("Area data for area " + areaNumber + " not found.");
            return;
        }

        // Convert the robotsDropdownValue to an integer
        int numberOfAgentsToPlace = int.Parse(areaData.robotsDropdownValue);
        List<Cell> availableCells = areaCells[areaNumber].Where(cell => cell.IsAvailable && !cell.IsBorder).ToList();

        for (int i = 0; i < numberOfAgentsToPlace; i++)
        {
            if (availableCells.Count > 0)
            {
                // Choose a random available cell for the agent
                int randomIndex = UnityEngine.Random.Range(0, availableCells.Count);
                Cell chosenCell = availableCells[randomIndex];
                availableCells.RemoveAt(randomIndex); // Remove the chosen cell from available cells

                // Instantiate the agent prefab at the chosen cell's position
                Vector3 agentPosition = chosenCell.Position + Vector3.up * 0.2f; // Adjust height as needed
                GameObject agent = Instantiate(agentPrefab, agentPosition, Quaternion.identity, objectContainer.transform);
                agent.SetActive(true);
                agents.Add(agent);

                // Apply a tag to the agent based on its order
                string agentTag = $"robot{agents.Count}";
                agent.tag = agentTag; // Ensure these tags are pre-defined in your project's tag manager

                // Optionally, find and store the base_link Transform
                Transform baseLinkTransform = FindDeepChild(agent.transform, "base_link");
                if (baseLinkTransform != null)
                {
                    agentBaseLinks[agent] = baseLinkTransform;
                }

                // Mark the cell as occupied
                chosenCell.IsAvailable = false;
            }
            else
            {
                Debug.LogWarning("Not enough available cells to place all agents in area " + areaNumber);
                break; // Exit the loop if there are no available cells left
            }
        }
    }

    // Visualizes the cells in the Unity editor, organized by areas.
    void OnDrawGizmos() 
    {
        if (areaCells == null || areaCells.Count == 0) return;

        // Iterate through each area in the dictionary
        foreach (KeyValuePair<int, List<Cell>> areaEntry in areaCells)
        {
            List<Cell> cellsInArea = areaEntry.Value;

            // Now iterate through each cell in the current area
            foreach (Cell cell in cellsInArea) 
            {
                Gizmos.color = cell.IsAvailable ? Color.green : Color.red;
                float cellSize = 1.0f; // Adjust this value as needed to match your actual cell size

                // Draw the cell using Gizmos
                DrawCellGizmos(cell, cellSize);
            }
        }
    }

    // Draws gizmos for a single cell.
    void DrawCellGizmos(Cell cell, float cellSize)
    {

        Gizmos.color = cell.IsBorder ? Color.red : Color.green;
        Vector3 topLeft = cell.Position + new Vector3(-cellSize/2, 0, cellSize/2);
        Vector3 topRight = cell.Position + new Vector3(cellSize/2, 0, cellSize/2);
        Vector3 bottomLeft = cell.Position + new Vector3(-cellSize/2, 0, -cellSize/2);
        Vector3 bottomRight = cell.Position + new Vector3(cellSize/2, 0, -cellSize/2);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }

// Function to create a single pillar at a specified position
void CreatePillarAtPosition(Vector3 position, Transform parent)
{
    // Assume you have a prefab or a method to create a pillar
    GameObject pillar = Instantiate(pillarPrefab, position, Quaternion.identity, parent);
    // Set any other properties of the pillar here
    pillar.transform.localScale = new Vector3(pillar.transform.localScale.x, pillar.transform.localScale.y*0.65f, pillar.transform.localScale.z);
}
    Transform FindDeepChild(Transform aParent, string aName)
    {
        foreach(Transform child in aParent)
        {
            if(child.name == aName )
            return child;
            Transform result = FindDeepChild(child, aName);
            if (result != null)
            return result;
        }
        return null;
    }

    // Function to create a floor tile using a prefab and adjusting its scale
    GameObject CreateFloorTile(Vector3 position)
    {
        // Instantiate the floor tile prefab at the given position with no rotation
        GameObject floorTileObj = Instantiate(floorContainerPrefab, position, Quaternion.identity);

        // Adjust the scale of the instantiated floor tile to match the tileSize
        floorTileObj.transform.localScale = tileSize;

        return floorTileObj;
    }
    // Function to create floor collider
    void CreateFloorCollider()
    {
        // Code to create floor collider
    }

    // Initializes containers for different room elements.
    void InitializeContainers()
    {
        wallContainer = new GameObject("WallContainer");
        objectContainer = new GameObject("ObjectContainer");
        // floorContainer = new GameObject("FloorContainer");
    }

    Color GetColorFromString(string colorName)
    {
        switch (colorName.ToLower())
        {
            case "green":
                return Color.green;
            case "pink":
                return new Color(1f, 0.75f, 0.8f); // Unity does not have a predefined 'pink'
            case "red":
                return Color.red;
            case "yellow":
                return Color.yellow;
            case "purple":
                return new Color(0.5f, 0f, 0.5f); // Unity does not have a predefined 'purple'
            case "orange":
                return new Color(1f, 0.64f, 0f); // Unity does not have a predefined 'orange'
            case "blue":
                return Color.blue;
            default:
                Debug.LogError("Unknown color: " + colorName);
                return Color.white; // Fallback color
        }
    }
    // Helper method to choose the correct door prefab based on door type
    GameObject GetDoorPrefab(string doorType)
    {
        switch (doorType.ToLower())
        {
            case "narrow door":
                return narrowDoorPrefab;
            case "wide door":
                return wideDoorPrefab;
            case "narrow blocked door":
                return narrowBlockedDoorPrefab;
            case "wide blocked door":
                return wideBlockedDoorPrefab;
            default:
                Debug.LogWarning("Unknown door type: " + doorType);
                return null; // Or a default door prefab
        }
    }

    void ChangeDoorColor(GameObject door, string colorName)
{
    Color color = GetColorFromString(colorName);
    var propertyBlock = new MaterialPropertyBlock();
    propertyBlock.SetColor("_Color", color);

    foreach (Transform child in door.transform)
    {
        var renderer = child.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.SetPropertyBlock(propertyBlock);
            child.gameObject.tag = colorName; // Assign tag based on the color name
        }
    }
}
}
