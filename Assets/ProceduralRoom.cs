using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std; // Import the standard message type for Int32MultiArray

// Represents a cell in the procedural room environment.
public class Cell
{
    public Vector3 Position { get; private set; }
    public bool IsAvailable { get; set; }
    public bool IsBorder { get; private set; }

    public Cell(Vector3 position, bool isAvailable = true, bool isBorder = false)
    {
        Position = position;
        IsAvailable = isAvailable;
        IsBorder = isBorder;
    }
}

public class ProceduralRoom : MonoBehaviour
{
    public SAData saData; // Reference to the MAData ScriptableObject
    public Vector2 roomSize;
    public Mesh wallMeshA, wallMeshB, wallMeshC;
    public Mesh pillarMesh, floorTile;
    public Material wallMaterial0, wallMaterial1;

    public Collider leftClawCollider;
    public Collider rightClawCollider;

    private List<Matrix4x4> wallMatricesA = new List<Matrix4x4>();
    private List<Matrix4x4> wallMatricesB = new List<Matrix4x4>();
    private List<Matrix4x4> wallMatricesC = new List<Matrix4x4>();

    private List<Matrix4x4> pillarMatrices = new List<Matrix4x4>();
    private List<Matrix4x4> floorMatrices = new List<Matrix4x4>();

    // Specify the correct size for your meshes here.
    public Vector3 wallSize = new Vector3(1, 1, 1);
    public Vector3 pillarSize = new Vector3(1, 1, 1);
    public Vector3 tileSize = new Vector3(0.25f, 0.25f, 0.25f);

    // Variables to store the previous state for change detection.
    private Vector2 previousRoomSize = new Vector2();
    private Vector3 previousPosition = new Vector3();
    private Quaternion previousRotation = new Quaternion();
    private Vector3 previousScale = new Vector3();

    private int seed;

    private List<Cell> cells = new List<Cell>();
    public GameObject[] objectPrefabs;
    private List<GameObject> instancedObjects = new List<GameObject>();
    private List<GameObject> agents = new List<GameObject>(); // Lista para manter os agentes

    public GameObject agentPrefab;
    public int numberOfAgents = 1;

    public GameObject redAreaPrefab;
    public GameObject currentBall;
    public GameObject greenAreaPrefab;
    public int numberOfRedAreas = 1;
    public int numberOfGreenAreas = 1;

    // Containers for different room elements.
    private GameObject wallContainer;
    private GameObject objectContainer;
    private GameObject floorContainer;

    public float floorHeight = 0.5f; // Floor height.

    private List<GameObject> wallObjects = new List<GameObject>();

    // Add a dictionary to hold references to the base_link of each agent
    private Dictionary<GameObject, Transform> agentBaseLinks = new Dictionary<GameObject, Transform>();

    public string robotToBallDistanceTopic = "robot_to_ball_distance";
    public string robotToGreenAreaDistanceTopic = "robot_to_green_area_distance";
    public string ballPossessionTopic = "ball_possession";

    private List<GameObject> greenAreas = new List<GameObject>();
    private List<GameObject> balls = new List<GameObject>(); // Lista para rastrear as bolas

    // ROS Connector
    ROSConnection ros;

    public int numberOfObjects = 10;

    // Places agents in the room.
    void PlaceAgents() 
    {
        // Check for available cells before attempting to place an agent.
        if (!cells.Any(c => c.IsAvailable && !c.IsBorder)) 
        {
            return;
        }

        for (int i = 0; i < numberOfAgents; i++) 
        {
            ForcePlaceAgent();
        }
    }

    // Moves an agent to a new position within the room.
    private void MoveAgentToNewPosition(GameObject agent)
    {
        const int maxAttempts = 20; // Maximum number of attempts to find an available cell.
        for (int attempt = 0; attempt < maxAttempts; attempt++) 
        {
            Cell randomCell = cells[Random.Range(0, cells.Count)];
            if (!randomCell.IsAvailable || randomCell.IsBorder) 
            {
                continue; // Retry if the cell is not available or is a border.
            }

            // Move the agent to the new position.
            agent.transform.position = randomCell.Position + Vector3.up * 0.2f; // Adjust the height as needed.
            randomCell.IsAvailable = false;
            return; // Exit the loop after successfully moving the agent.
        }
    }

    // Creates special red and green areas in the room.
    void CreateSpecialAreas()
    {
        greenAreas.Clear(); // Limpa a lista antes de criar novas áreas

        for (int i = 0; i < numberOfRedAreas; i++)
        {
            GameObject redArea = PlaceObjectInRandomCell(redAreaPrefab);
            if (redArea != null)
            {
                SpawnBall spawnBallScript = redArea.GetComponent<SpawnBall>();
                if (spawnBallScript != null)
                {
                    spawnBallScript.OnBallSpawned += BallSpawned;
                }
            }
        }
        for (int i = 0; i < numberOfGreenAreas; i++)
        {
            GameObject greenArea = PlaceObjectInRandomCell(greenAreaPrefab);
            if (greenArea != null)
            {
                greenAreas.Add(greenArea); // Adiciona a instância à lista
            }
        }
    }

    // Helper method to place an object in a random cell.
    GameObject PlaceObjectInRandomCell(GameObject prefab, float heightOffset = 0.03f)
    {
        const int maxAttempts = 10;
        for (int attempt = 0; attempt < maxAttempts; attempt++) 
        {
            Cell randomCell = cells[Random.Range(0, cells.Count)];
            if (!randomCell.IsAvailable || randomCell.IsBorder) 
            {
                continue;
            }

            Vector3 spawnPosition = randomCell.Position + Vector3.up * heightOffset;
            GameObject instantiatedObject = Instantiate(prefab, spawnPosition, Quaternion.identity, objectContainer.transform);
            instantiatedObject.SetActive(true);
            instancedObjects.Add(instantiatedObject);

            randomCell.IsAvailable = false;
            return instantiatedObject; // Retorna o objeto instanciado.
        }
        return null; // Retorna null se falhar.
    }

    // Forces the placement of an agent in the room.
    void ForcePlaceAgent() 
    {
        const int maxAttempts = 20; // Limit for attempts.
        for (int attempt = 0; attempt < maxAttempts; attempt++) 
        {
            Cell randomCell = cells[Random.Range(0, cells.Count)];

            if (randomCell.IsBorder || !randomCell.IsAvailable) 
            {
                continue; // Retry if cell is invalid.
            }

            Vector3 agentPosition = randomCell.Position + Vector3.up * 0.2f; // Adjust the height as needed.
            GameObject agent = Instantiate(agentPrefab, agentPosition, Quaternion.identity, objectContainer.transform);
            agent.SetActive(true);
            instancedObjects.Add(agent);
            agents.Add(agent);

            Transform baseLinkTransform = FindDeepChild(agent.transform, "base_link");
            if (baseLinkTransform != null)
            {
                agentBaseLinks[agent] = baseLinkTransform;
            }
            else
            {
                Debug.LogError("base_link not found in the agent prefab");
            }

            randomCell.IsAvailable = false; // Mark the cell as occupied.
            return; // Exit the loop after successful instantiation.
        }

        Debug.LogError("Unable to position the agent after multiple attempts.");
    }

    // Checks if there have been any changes to the room size or transform.
    bool anyChanges()
    {
        // Check if the room size has changed.
        bool sizeChanged = previousRoomSize != roomSize;
        
        // Check if the position, rotation, or scale of the transform has changed.
        bool transformChanged = previousPosition != transform.position ||
                                previousRotation != transform.rotation ||
                                previousScale != transform.localScale;
        
        // Update the previous state for the next check.
        previousRoomSize = roomSize;
        previousPosition = transform.position;
        previousRotation = transform.rotation;
        previousScale = transform.localScale;

        // Return true if there were any changes.
        return sizeChanged || transformChanged;
    }

    // Places random objects with colliders in the room.
    void PlaceRandomObjectsWithColliders(int numberOfObjects) 
    {
        Random.InitState(seed);

        int cellCount = cells.Count;
        int availableCells = cellCount - cells.Count(cell => cell.IsBorder);
        numberOfObjects = Mathf.Min(numberOfObjects, availableCells);

        for (int i = 0, attempts = 0; i < numberOfObjects && attempts < cellCount * 2; attempts++) 
        {
            Cell randomCell = cells[Random.Range(0, cellCount)];
            if (randomCell.IsBorder || !randomCell.IsAvailable) continue;

            GameObject prefab = objectPrefabs[Random.Range(0, objectPrefabs.Length)];       
            Quaternion rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            GameObject obj = Instantiate(prefab, randomCell.Position, rotation, objectContainer.transform);
            
            instancedObjects.Add(obj);
            AddMeshCollider(obj);
            
            obj.tag = "Object";
            obj.layer = LayerMask.NameToLayer("Object");

            randomCell.IsAvailable = false;
            i++;
        }
    }

    // Adds a mesh collider to the object if it doesn't have one.
    void AddMeshCollider(GameObject obj) 
    {
        if (obj == null) return;

        MeshCollider collider = obj.GetComponent<MeshCollider>();
        MeshFilter filter = obj.GetComponent<MeshFilter>();

        if (collider == null && filter != null && filter.sharedMesh != null) 
        {
            collider = obj.AddComponent<MeshCollider>();
            collider.sharedMesh = filter.sharedMesh;
        }
    }

    // Clears all dynamically created objects in the room.
    void ClearObjects()
    {
        // Lógica existente para limpar objetos
        foreach (GameObject obj in agents)
        {
            if (obj != null) Destroy(obj);
        }
        agents.Clear();
        agentBaseLinks.Clear(); // Limpa o dicionário também

        foreach (GameObject obj in instancedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        instancedObjects.Clear();

        foreach (GameObject wall in wallObjects) 
        {
            if (wall != null) Destroy(wall);
        }
        wallObjects.Clear();

        // Nova lógica para destruir as bolas
        foreach (GameObject ball in balls)
        {
            if (ball != null) Destroy(ball);
        }
        balls.Clear(); // Limpa a lista de bolas
    }

    // Creates a grid of cells based on room size.
    void CreateCells() 
    {
        cells.Clear();
        float cellSize = 2.0f;
        int cellCountX = Mathf.FloorToInt(roomSize.x / cellSize);
        int cellCountY = Mathf.FloorToInt(roomSize.y / cellSize);

        for (int x = 0; x < cellCountX; x++) 
        {
            for (int y = 0; y < cellCountY; y++) 
            {
                Vector3 position = transform.position + new Vector3(-roomSize.x / 2 + cellSize / 2 + x * cellSize, 0, -roomSize.y / 2 + cellSize / 2 + y * cellSize);
                bool isBorder = x == 0 || x == cellCountX - 1 || y == 0 || y == cellCountY - 1;
                cells.Add(new Cell(position, true, isBorder));
            }
        }
    }

    // Visualizes the cells in the Unity editor.
    void OnDrawGizmos() 
    {
        if (cells == null || cells.Count == 0) return;

        foreach (Cell cell in cells) 
        {
            Gizmos.color = cell.IsAvailable ? Color.green : Color.red;
            float cellSize = 2.0f; 
            DrawCellGizmos(cell, cellSize);
        }
    }

    // Draws gizmos for a single cell.
    void DrawCellGizmos(Cell cell, float cellSize)
    {
        Vector3 topLeft = cell.Position + new Vector3(-cellSize/2, 0, cellSize/2);
        Vector3 topRight = cell.Position + new Vector3(cellSize/2, 0, cellSize/2);
        Vector3 bottomLeft = cell.Position + new Vector3(-cellSize/2, 0, -cellSize/2);
        Vector3 bottomRight = cell.Position + new Vector3(cellSize/2, 0, -cellSize/2);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }

    // Initialization logic executed at the start.
    void Start() 
    {
        // Check if MAData is assigned
        if (saData == null)
        {
            Debug.LogError("SAData reference is not assigned!");
            return;
        }
        roomSize = saData.roomSize;
        
        roomSize = saData.roomSize;
        numberOfRedAreas = saData.numberOfRedAreas;
        numberOfGreenAreas = saData.numberOfGreenAreas;

        numberOfObjects = saData.numberOfObjects;

        seed = Random.Range(0, int.MaxValue);

        InitializeContainers();

        CreateRoom();
        PlaceAgents();
        CreateSpecialAreas();
        PlaceRandomObjectsWithColliders(numberOfObjects); 
        AdjustObjectHeights();

        // Encontre os GameObjects das garras pelo nome ou tag e atribua os colliders
        GameObject leftClawGameObject = GameObject.Find("yamabiko_claw_left"); // Substitua pelo nome exato no seu projeto
        if (leftClawGameObject != null)
        {
            leftClawCollider = leftClawGameObject.GetComponent<Collider>();
        }
        else
        {
            Debug.LogError("GameObject da garra esquerda não encontrado");
        }

        GameObject rightClawGameObject = GameObject.Find("yamabiko_claw_right"); // Substitua pelo nome exato no seu projeto
        if (rightClawGameObject != null)
        {
            rightClawCollider = rightClawGameObject.GetComponent<Collider>();
        }
        else
        {
            Debug.LogError("GameObject da garra direita não encontrado");
        }

        // Verifique se ambos os colliders foram atribuídos
        if (leftClawCollider == null || rightClawCollider == null)
        {
            Debug.LogError("Um ou ambos os colliders das garras não foram atribuídos");
        }


        // Get or create the ROS Connection instance
        ros = ROSConnection.GetOrCreateInstance();
        // Subscribe to the /reset topic
        ros.Subscribe<Int32MultiArrayMsg>("reset", ResetRoomSizeCallback);
        ros.RegisterPublisher<Float32Msg>(robotToBallDistanceTopic);
        ros.RegisterPublisher<Float32Msg>(robotToGreenAreaDistanceTopic);
        ros.RegisterPublisher<BoolMsg>(ballPossessionTopic);

        //SpawnBall spawnBallScript = redAreaPrefab.GetComponent<SpawnBall>();
        //if (spawnBallScript != null)
        //{
        //    spawnBallScript.OnBallSpawned += BallSpawned;
        //}
    }

    // Update logic executed every frame.
    void Update() 
    {
        // Use current time as the seed for random number generator
        seed = (int)(Time.time * 1000);
        Random.InitState(seed);

        if (anyChanges()) // Corrected the method name to match its definition
        {
            CreateRoom();
            PlaceAgents();
            CreateSpecialAreas();
            PlaceRandomObjectsWithColliders(numberOfObjects); 
            AdjustObjectHeights();
        }

        // Encontre os GameObjects das garras pelo nome ou tag e atribua os colliders
        GameObject leftClawGameObject = GameObject.Find("yamabiko_claw_left"); // Substitua pelo nome exato no seu projeto
        if (leftClawGameObject != null)
        {
            leftClawCollider = leftClawGameObject.GetComponent<Collider>();
        }
        else
        {
            Debug.LogError("GameObject da garra esquerda não encontrado");
        }

        GameObject rightClawGameObject = GameObject.Find("yamabiko_claw_right"); // Substitua pelo nome exato no seu projeto
        if (rightClawGameObject != null)
        {
            rightClawCollider = rightClawGameObject.GetComponent<Collider>();
        }
        else
        {
            Debug.LogError("GameObject da garra direita não encontrado");
        }

        // Verifique se ambos os colliders foram atribuídos
        if (leftClawCollider == null || rightClawCollider == null)
        {
            Debug.LogError("Um ou ambos os colliders das garras não foram atribuídos");
        }


        if (agents.Count > 0 && currentBall != null && greenAreas.Count > 0)
        {
            GameObject agent = agents[0];
            Transform baseLinkTransform = agentBaseLinks[agent];
            float distanceToBall = Vector3.Distance(baseLinkTransform.position, currentBall.transform.position);

            // Assumindo que você quer verificar a distância para a primeira green area na lista
            GameObject greenAreaInstance = greenAreas[0];
            Collider greenAreaCollider = greenAreaInstance.GetComponent<Collider>();
            Vector3 greenAreaCenter = greenAreaCollider != null ? greenAreaCollider.bounds.center : greenAreaInstance.transform.position;

            float distanceToGreenArea = Vector3.Distance(baseLinkTransform.position, greenAreaCenter);
            bool isBallPossessed = distanceToBall < 1.0f;

            ros.Publish(robotToBallDistanceTopic, new Float32Msg(distanceToBall));
            ros.Publish(robotToGreenAreaDistanceTopic, new Float32Msg(distanceToGreenArea));
            ros.Publish(ballPossessionTopic, new BoolMsg(isBallPossessed));
        }

        if (currentBall != null && IsBallCaught())
        {
            // Publique a posse da bola se ela estiver presa
            PublishBallPossession(true);
        }
        else
        {
            // Se a bola não estiver presa, publique que não está possuída
            PublishBallPossession(false);
        }

        RenderRoom();
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

    // Creates the room elements.
    void CreateRoom() 
    {
        ClearObjects();
        CreateWallsWithColliders();
        CreatePillars();
        CreateFloor();
        CreateCells();
    }

    // Renders the room elements.
    void RenderRoom() 
    {
        RenderWalls();
        RenderPillars();
        RenderFloor();
    }

    private void BallSpawned(GameObject tennisBallPrefab)
    {
        // Adiciona a nova bola à lista de rastreamento
        balls.Add(tennisBallPrefab);

        // Use FindDeepChild to find the "ball" GameObject.
        Transform ballChildTransform = FindDeepChild(tennisBallPrefab.transform, "ball");
        if (ballChildTransform != null)
        {
            currentBall = ballChildTransform.gameObject;
        }
        else
        {
            Debug.LogError("Child 'ball' GameObject not found after instantiation.");
        }
    }

    private bool IsBallCaught()
    {
        // Ensure the currentBall and its Collider are valid and the Collider is enabled
        if (currentBall == null)
        {
            Debug.LogWarning("Tennis ball GameObject is null.");
            return false;
        }
        
        Collider ballCollider = currentBall.GetComponent<Collider>();
        if (ballCollider == null || !ballCollider.enabled)
        {
            Debug.LogWarning("Ball collider is missing or not enabled.");
            return false;
        }

        // Check for left and right claw colliders' existence and enabled state
        if (leftClawCollider == null || !leftClawCollider.enabled || rightClawCollider == null || !rightClawCollider.enabled)
        {
            Debug.LogWarning("One or both claw colliders are missing or not enabled.");
            return false;
        }

        // Now safely check if the ball is caught between the claws
        return leftClawCollider.bounds.Intersects(ballCollider.bounds) &&
            rightClawCollider.bounds.Intersects(ballCollider.bounds);
    }

    // Método para publicar a posse da bola
    private void PublishBallPossession(bool isPossessed)
    {
        ros.Publish(ballPossessionTopic, new BoolMsg(isPossessed));
    }

    // Callback function called when a message is received on the /reset topic
    private void ResetRoomSizeCallback(Int32MultiArrayMsg newSize)
    {
        if (newSize.data.Length >= 2)
        {
            // Update the room size and reset the room
            roomSize = new Vector2(newSize.data[0], newSize.data[1]);
            ResetRoom();
        }
    }

    // Function to reset the room with the new size
    private void ResetRoom()
    {
        // Clear existing objects and recreate the room
        ClearObjects();
        CreateCells();
        CreateRoom();
        PlaceAgents();
        CreateSpecialAreas();
        PlaceRandomObjectsWithColliders(numberOfObjects); // Number of objects to place
        AdjustObjectHeights();
    }

    // Initializes containers for different room elements.
    void InitializeContainers()
    {
        wallContainer = new GameObject("WallContainer");
        objectContainer = new GameObject("ObjectContainer");
        floorContainer = new GameObject("FloorContainer");
    }

    // Adjusts the heights of objects in the room.
    private void AdjustObjectHeights()
    {
        for (int i = 0; i < pillarMatrices.Count; i++)
        {
            Matrix4x4 matrix = pillarMatrices[i];
            AdjustMatrixHeight(ref matrix, floorHeight);
            pillarMatrices[i] = matrix; // Reassign the modified matrix back to the list
        }

        foreach (var obj in instancedObjects)
        {
            if (obj != null) obj.transform.position += Vector3.up * floorHeight;
        }

        foreach (GameObject wall in wallObjects) 
        {
            if (wall != null) AdjustGameObjectHeight(wall, floorHeight);
        }
    }

    // Adjusts the height of a matrix.
    void AdjustMatrixHeight(ref Matrix4x4 matrix, float height)
    {
        var position = matrix.GetColumn(3);
        matrix = Matrix4x4.TRS(new Vector3(position.x, position.y + height, position.z), matrix.rotation, matrix.lossyScale);
    }

    // Adjusts the height of a single game object.
    void AdjustGameObjectHeight(GameObject obj, float height)
    {
        Vector3 position = obj.transform.position;
        obj.transform.position = new Vector3(position.x, position.y + height, position.z);
    }

    // Creates walls with colliders for the room.
    void CreateWallsWithColliders() 
    {
        // Destroy old walls in the container
        foreach (Transform child in wallContainer.transform) 
        {
            Destroy(child.gameObject);
        }

        int wallCountX = Mathf.Max(1, Mathf.RoundToInt(roomSize.x / wallSize.x));
        int wallCountY = Mathf.Max(1, Mathf.RoundToInt(roomSize.y / wallSize.y));
        float scaleX = roomSize.x / wallCountX / wallSize.x;
        float scaleY = roomSize.y / wallCountY / wallSize.y;

        // Create walls along X and Y axis
        for (int i = 0; i < wallCountX; i++) 
        {
            CreateAndAddWall(i, scaleX, isHorizontal: true);
            CreateAndAddWall(i, scaleX, isHorizontal: true, includeCorners: false);
        }
        for (int i = 0; i < wallCountY; i++) 
        {
            CreateAndAddWall(i, scaleY, isHorizontal: false, includeCorners: false);
            CreateAndAddWall(i, scaleY, isHorizontal: false);
        }
    }

    // Helper function to create and add wall object.
    void CreateAndAddWall(int index, float scale, bool isHorizontal, bool includeCorners = true)
    {
        GameObject wall = CreateWallObject(index, scale, isHorizontal, includeCorners);
        wallObjects.Add(wall);
    }

    // Creates a single wall object.
    GameObject CreateWallObject(int index, float scale, bool isHorizontal, bool includeCorners = true) 
    {
        Vector3 positionOffset;
        Vector3 scaleVector = new Vector3(scale, 1, 1);
        Quaternion rotation = transform.rotation;

        // Choose a random mesh for the wall
        Mesh chosenMesh = ChooseRandomWallMesh();

        // Positioning and scaling for horizontal and vertical walls
        if (isHorizontal) 
        {
            positionOffset = new Vector3(-roomSize.x / 2 + wallSize.x * scale / 2 + index * scale * wallSize.x, 0, includeCorners ? -roomSize.y / 2 : roomSize.y / 2);
        } 
        else 
        {
            positionOffset = new Vector3(includeCorners ? -roomSize.x / 2 : roomSize.x / 2, 0, -roomSize.y / 2 + wallSize.y * scale / 2 + index * scale * wallSize.y);
            scaleVector = new Vector3(scale, 1, 1);
            rotation *= Quaternion.Euler(0, 90, 0); // Rotate Y axis walls by 90 degrees
        }

        GameObject wallObj = new GameObject("Wall");
        wallObj.tag = "Wall";
        wallObj.layer = LayerMask.NameToLayer("Wall");
        wallObj.transform.SetParent(wallContainer.transform);
        wallObj.transform.position = transform.position + positionOffset;
        wallObj.transform.localScale = scaleVector;
        wallObj.transform.rotation = rotation;

        // Adding MeshRenderer and MeshFilter
        MeshRenderer meshRenderer = wallObj.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = wallObj.AddComponent<MeshFilter>();
        meshFilter.mesh = chosenMesh;
        meshRenderer.material = wallMaterial0;

        // Adding MeshCollider
        // MeshCollider meshCollider = wallObj.AddComponent<MeshCollider>();
        // meshCollider.sharedMesh = meshFilter.mesh;
        // meshCollider.convex = false; // Set to false to allow non-convex colliders

        // Adding MeshCollider
        if (chosenMesh.isReadable) // Verifique se o mesh está acessível
        {
            MeshCollider meshCollider = wallObj.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = chosenMesh;
            meshCollider.convex = false;
        }
        else
        {
            Debug.LogError("Mesh is not readable: " + chosenMesh.name);
        }

        return wallObj;
    }

    // Chooses a random wall mesh.
    Mesh ChooseRandomWallMesh() 
    {
        int randomIndex = Random.Range(0, 3);
        return randomIndex switch
        {
            0 => wallMeshA,
            1 => wallMeshB,
            2 => wallMeshC,
            _ => wallMeshA,
        };
    }

    // Creates pillars for the room.
    void CreatePillars() 
    {
        pillarMatrices.Clear();
        // Corner positions for the pillars
        Vector3[] cornerPositions = 
        {
            new Vector3(-roomSize.x / 2, 0, -roomSize.y / 2),
            new Vector3(-roomSize.x / 2, 0, roomSize.y / 2),
            new Vector3(roomSize.x / 2, 0, -roomSize.y / 2),
            new Vector3(roomSize.x / 2, 0, roomSize.y / 2)
        };

        foreach (Vector3 corner in cornerPositions) 
        {
            Vector3 position = transform.position + corner;
            pillarMatrices.Add(Matrix4x4.TRS(position, Quaternion.identity, Vector3.one));
        }
    }

    // Creates the floor of the room.
    void CreateFloor() 
    {
        // Destroy old container and create a new one
        if (floorContainer != null) 
        {
            Destroy(floorContainer);
        }
        floorContainer = new GameObject("FloorContainer");
        floorMatrices.Clear();

        // Adjust the spacing between tiles
        float spacingX = tileSize.x * 4;
        float spacingY = tileSize.y * 4;

        int floorCountX = Mathf.Max(1, Mathf.RoundToInt(roomSize.x / spacingX));
        int floorCountY = Mathf.Max(1, Mathf.RoundToInt(roomSize.y / spacingY));

        for (int i = 0; i < floorCountX; i++) 
        {
            for (int j = 0; j < floorCountY; j++) 
            {
                Vector3 position = transform.position + new Vector3(-roomSize.x / 2 + spacingX / 2 + i * spacingX, 0, -roomSize.y / 2 + spacingY / 2 + j * spacingY);
                GameObject floorTileObj = CreateFloorTile(position);
                floorTileObj.transform.parent = floorContainer.transform;
            }
        }
        
        CreateFloorCollider();
    }

    // Creates a single floor tile.
    GameObject CreateFloorTile(Vector3 position)
    {
        GameObject floorTileObj = new GameObject("FloorTile");
        floorTileObj.transform.position = position;
        floorTileObj.transform.localScale = tileSize;

        MeshRenderer meshRenderer = floorTileObj.AddComponent<MeshRenderer>();
        meshRenderer.material = wallMaterial1;

        MeshFilter meshFilter = floorTileObj.AddComponent<MeshFilter>();
        meshFilter.mesh = floorTile;

        return floorTileObj;
    }

    // Creates a collider for the floor.
    void CreateFloorCollider() 
    {
        GameObject floorColliderObj = new GameObject("FloorCollider");
        floorColliderObj.transform.SetParent(floorContainer.transform);
        floorColliderObj.transform.position = transform.position + Vector3.down * 0.05f; // Position slightly below the visual floor to avoid z-fighting

        BoxCollider boxCollider = floorColliderObj.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(roomSize.x, 0.35f, roomSize.y); // Small height for the collider to act as a "plane"

        PhysicMaterial floorMaterial = new PhysicMaterial("FloorMaterial");
        floorMaterial.dynamicFriction = 0.6f;
        floorMaterial.staticFriction = 0.6f;

        boxCollider.material = floorMaterial;
    }

    // Renders the walls of the room.
    void RenderWalls() 
    {
        RenderWallType(wallMatricesA, wallMeshA);
        RenderWallType(wallMatricesB, wallMeshB);
        RenderWallType(wallMatricesC, wallMeshC);
    }

    // Helper method to render a specific type of wall.
    void RenderWallType(List<Matrix4x4> matrices, Mesh mesh)
    {
        if (matrices.Count > 0) 
        {
            Graphics.DrawMeshInstanced(mesh, 0, wallMaterial0, matrices.ToArray(), matrices.Count);
        }
    }

    // Renders the pillars of the room.
    void RenderPillars() 
    {
        foreach (var matrix in pillarMatrices) 
        {
            Graphics.DrawMesh(pillarMesh, matrix, wallMaterial0, 0);
        }
    }

    // Renders the floor of the room.
    void RenderFloor() 
    {
        foreach (var matrix in floorMatrices) 
        {
            Graphics.DrawMesh(floorTile, matrix, wallMaterial1, 0);
        }
    }
}
