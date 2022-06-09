using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance{set;get;}
    private bool[,] allowedMoves{set;get;}


    public Chessman[,] Chessmans{set;get;}
    private Chessman selectedChessman;

    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;

    private int selectionX = -1;
    private int selectionY = -1;

    public List<GameObject> chessmanPrefabs;
    private List<GameObject> activeChessman = new List<GameObject>();
    
    private Material previousMat;
    public Material selectedMat;

    public int[] EnPassantMove{set;get;}

    private Quaternion orientation = Quaternion.Euler(0, 180, 0);

    public bool isWhiteTurn = true;

    Camera m_MainCamera;
    public Camera cam2;

    private Chessman castlingChessman;
    //0:white king moved, 1:black king moved, 2:right white rook move
    //3:left white rook move, 4:left black rook move, 5:right black rook move
    public bool[] castlingMove{set;get;}
     
    private void Start()
    {
        m_MainCamera = Camera.main;
        cam2 = GameObject.Find("Camera2").GetComponent<Camera>();

        m_MainCamera.enabled = true;
        cam2.enabled = false;
        Instance = this;
        SpawnAllChessmans();
    }

    private void Update()
    {
        UpdateSelection();
        DrawChessBoard();

        if(Input.GetMouseButtonDown(0)) {
            if (selectionX >= 0 && selectionY >= 0) {
                if (selectedChessman == null) {
                    //Select the chessman
                    SelectChessman(selectionX, selectionY);

                } else {
                    //Move the chessman
                    MoveChessman(selectionX, selectionY);

                }
            }
        }
    }

    private void SelectChessman(int x, int y) {
        if (Chessmans[x, y] == null)
            return;

        if (Chessmans[x,y].isWhite != isWhiteTurn)
            return;
        
        bool hasAtLeastOneMove = false;
        allowedMoves = Chessmans[x, y].PossibleMove();

        for (int i = 0; i < 8; i++) {
            for (int j = 0; j < 8; j++) {
                if (allowedMoves[i,j])
                    hasAtLeastOneMove = true;
            }
        }

        if (!hasAtLeastOneMove)
            return;

        
        selectedChessman = Chessmans[x, y];
        previousMat = selectedChessman.GetComponent<MeshRenderer>().material;
        selectedMat.mainTexture = previousMat.mainTexture;
        selectedChessman.GetComponent<MeshRenderer>().material = selectedMat;

        BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);
    

    }

    private void MoveChessman(int x, int y) 
    {
        if (allowedMoves[x, y]) {
            Chessman c = Chessmans[x, y];
            if (c != null && c.isWhite != isWhiteTurn) {
                //Capture a piece

                //If it is the king
                if (c.GetType() == typeof(King)) {
                    //End the game
                    EndGame();
                    return;
                }

                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);

            }

            if (x == EnPassantMove[0] && y == EnPassantMove[1]) {
                if (isWhiteTurn) {
                    c = Chessmans[x, y-1];
                } else {
                    c = Chessmans[x, y+1];
                }
                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
            }
            EnPassantMove[0] = -1;
            EnPassantMove[1] = -1;
            if (selectedChessman.GetType() == typeof(Pawn)) {
                if (y == 7) {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChessman(1, x, y, Quaternion.Euler(0, 0, 0));
                    selectedChessman = Chessmans[x, y];
                }
                if (y == 0) {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChessman(7, x, y, Quaternion.Euler(0, 0, 0));
                    selectedChessman = Chessmans[x, y];
                }

                if (selectedChessman.CurrentY == 1 && y == 3) {
                    EnPassantMove[0] = x;
                    EnPassantMove[1] = y - 1;

                } else if (selectedChessman.CurrentY == 6 && y == 4) {
                    EnPassantMove[0] = x;
                    EnPassantMove[1] = y + 1;

                }
            }

            if (selectedChessman.GetType() == typeof(Rook)) {
                if (isWhiteTurn) {
                    if (selectedChessman.CurrentX == 7 && selectedChessman.CurrentY == 0)
                        castlingMove[2] = false;
                    if (selectedChessman.CurrentX == 0 && selectedChessman.CurrentY== 0)
                        castlingMove[3] = false;
                } else {
                    if (selectedChessman.CurrentX == 0 && selectedChessman.CurrentY == 7)
                        castlingMove[4] = false;
                    if (selectedChessman.CurrentX == 7 && selectedChessman.CurrentY == 7)
                        castlingMove[5] = false;
                }
            }

            if (selectedChessman.GetType() == typeof(King)) {
                // Neither king or rook has moved
                if (isWhiteTurn)
                    castlingMove[0] = false;
                else
                    castlingMove[1] = false;

                //White right
                if (selectedChessman.CurrentX == 3 && x == 5 && isWhiteTurn) {
                    castlingChessman = Chessmans[7, 0];
                    Chessmans[castlingChessman.CurrentX, castlingChessman.CurrentY] = null;
                    castlingChessman.transform.position = GetTileCenter(x-1, y);
                    castlingChessman.SetPosition(x-1, y);
                    Chessmans[x-1, y] = castlingChessman;
                }

                //White left
                else if (selectedChessman.CurrentX == 3 && x == 1 && isWhiteTurn) {
                    castlingChessman = Chessmans[0, 0];
                    Chessmans[castlingChessman.CurrentX, castlingChessman.CurrentY] = null;
                    castlingChessman.transform.position = GetTileCenter(x+1, y);
                    castlingChessman.SetPosition(x+1, y);
                    Chessmans[x+1, y] = castlingChessman;
                }

                //Black right
                else if (selectedChessman.CurrentX == 4 && x == 6 && !isWhiteTurn) {
                    castlingChessman = Chessmans[7, 7];
                    Chessmans[castlingChessman.CurrentX, castlingChessman.CurrentY] = null;
                    castlingChessman.transform.position = GetTileCenter(x-1, y);
                    castlingChessman.SetPosition(x-1, y);
                    Chessmans[x-1, y] = castlingChessman;
                }

                //Black left
                else if (selectedChessman.CurrentX == 4 && x == 2 && !isWhiteTurn) {
                    castlingChessman = Chessmans[0, 7];
                    Chessmans[castlingChessman.CurrentX, castlingChessman.CurrentY] = null;
                    castlingChessman.transform.position = GetTileCenter(x+1, y);
                    castlingChessman.SetPosition(x+1, y);
                    Chessmans[x+1, y] = castlingChessman;
                }
            }

            Chessmans[selectedChessman.CurrentX, selectedChessman.CurrentY] = null;
            selectedChessman.transform.position = GetTileCenter(x, y);
            selectedChessman.SetPosition(x, y);
            Chessmans[x, y] = selectedChessman;
            isWhiteTurn = !isWhiteTurn;

            m_MainCamera.enabled = !m_MainCamera.enabled;
            cam2.enabled = !cam2.enabled;
            if (m_MainCamera.enabled) {
                m_MainCamera.targetDisplay = 0;
                cam2.targetDisplay = 1;
            } else if (cam2.enabled) {
                cam2.targetDisplay = 0;
                m_MainCamera.targetDisplay = 1;
            }

        }


        selectedChessman.GetComponent<MeshRenderer>().material = previousMat;
        BoardHighlights.Instance.Hidehighlights();
        selectedChessman = null;
    }

    private void UpdateSelection()
    {
        if (m_MainCamera.enabled) {
            RaycastHit hit;
            if (Physics.Raycast(m_MainCamera.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("ChessPlane"))) {
                selectionX = (int)hit.point.x;
                selectionY = (int)hit.point.z;
            }
            else {
                selectionX = -1;
                selectionY = -1;
            }   
        } else if (cam2.enabled) {
            RaycastHit hit;
            if (Physics.Raycast(cam2.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("ChessPlane"))) {
                selectionX = (int)hit.point.x;
                selectionY = (int)hit.point.z;
            }
            else {
                selectionX = -1;
                selectionY = -1;
            }   
        } else {
            selectionX = -1;
            selectionY = -1;
        }
        
        

    }

    private void DrawChessBoard()
    {
        Vector3 widthLine = Vector3.right * 8;
        Vector3 heightLine = Vector3.forward * 8;
        /*
        for (int i = 0; i <= 8; i++) {
            Vector3 start = Vector3.forward * i;
            Debug.DrawLine(start, start + widthLine);
            for (int j = 0; j <= 8; j++) {
                start = Vector3.right * j;
                Debug.DrawLine(start, start + heightLine);

            }
        }
        */

        // Draw the selection
        if (selectionX >= 0 && selectionY >= 0) {
            Debug.DrawLine(
                Vector3.forward * selectionY + Vector3.right * selectionX,
                Vector3.forward * (selectionY + 1) + Vector3.right * (selectionX + 1));
            
            Debug.DrawLine(
                Vector3.forward * (selectionY + 1) + Vector3.right * selectionX,
                Vector3.forward * selectionY + Vector3.right * (selectionX + 1));
        }
    }

    private void SpawnChessman(int index, int x, int y, Quaternion orientation)
    {
        GameObject go = Instantiate(chessmanPrefabs[index], GetTileCenter(x, y), orientation) as GameObject;
        go.transform.SetParent(transform);
        Chessmans[x,y] = go.GetComponent<Chessman> ();
        Chessmans[x,y].SetPosition(x, y);
        activeChessman.Add(go);
    }

    private void SpawnAllChessmans()
    {
        activeChessman = new List<GameObject> ();
        Chessmans = new Chessman[8,8];
        EnPassantMove = new int[2]{-1, -1};
        castlingMove = new bool[6]{true, true, true, true, true, true};

        // Spawn the white team!

        //King
        SpawnChessman(0, 3, 0, Quaternion.Euler(0, 0, 0));

        //Queen
        SpawnChessman(1, 4, 0, Quaternion.Euler(0, 0, 0));

        //Rooks
        SpawnChessman(2, 0, 0, Quaternion.Euler(0, 0, 0));
        SpawnChessman(2, 7, 0, Quaternion.Euler(0, 0, 0));

        //Bishops
        SpawnChessman(3, 2, 0, Quaternion.Euler(0, 270, 0));
        SpawnChessman(3, 5, 0, Quaternion.Euler(0, 270, 0));

        //Knights
        SpawnChessman(4, 1, 0, Quaternion.Euler(0, 0, 0));
        SpawnChessman(4, 6, 0, Quaternion.Euler(0, 0, 0));

        //Pawns
        for (int i = 0; i < 8; i++) {
            SpawnChessman(5, i, 1, Quaternion.Euler(0, 0, 0));
        }

        // Spawn the black team!

        //King
        SpawnChessman(6, 4, 7, Quaternion.Euler(0, 0, 0));

        //Queen
        SpawnChessman(7, 3, 7, Quaternion.Euler(0, 0, 0));

        //Rooks
        SpawnChessman(8, 0, 7, Quaternion.Euler(0, 0, 0));
        SpawnChessman(8, 7, 7, Quaternion.Euler(0, 0, 0));

        //Bishops
        SpawnChessman(9, 2, 7, Quaternion.Euler(0, 270, 0));
        SpawnChessman(9, 5, 7, Quaternion.Euler(0, 270, 0));

        //Knights
        SpawnChessman(10, 1, 7, Quaternion.Euler(0, 0, 0));
        SpawnChessman(10, 6, 7, Quaternion.Euler(0, 0, 0));

        //Pawns
        for (int i = 0; i < 8; i++) {
            SpawnChessman(11, i, 6, Quaternion.Euler(0, 0, 0));
        }
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        origin.z += (TILE_SIZE * y) + TILE_OFFSET;
        return origin;
    }

    private void EndGame()
    {
        if (isWhiteTurn) {
            Debug.Log("White team wins");
        } else {
            Debug.Log("Black team wins");
        }
        
        foreach(GameObject go in activeChessman)
            Destroy(go);

        isWhiteTurn = true;
        BoardHighlights.Instance.Hidehighlights();
        SpawnAllChessmans();

        m_MainCamera.enabled = true;
        cam2.enabled = false;
        m_MainCamera.targetDisplay = 0;
        cam2.targetDisplay = 1;

    }







}
