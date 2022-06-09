using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Chessman
{
    public override bool[,] PossibleMove()
    {
        bool[,] r = new bool[8, 8];

        Chessman c;
        bool[] e = BoardManager.Instance.castlingMove;

        int i, j;
        bool castle = true;

        //Castling Right
        i = CurrentX;
        j = CurrentY;
        castle = true;
        if ((e[0] && isWhite) || (e[1] && !isWhite))
        {
            for (int k = 0; k < 4; k++) {
                c = BoardManager.Instance.Chessmans[i, j];
                if ((c == null || k == 0) && i < 7 && i > 0) {
                    if (isWhite) {
                        if (e[2]) {
                            i++;
                        } else {
                            castle = false;
                            break;
                        }
                    } else {
                        if (e[4]) {
                            i--;
                        } else {
                            castle = false;
                            break;
                        }
                    }

                } else {
                    castle = false;
                    break;
                }      

            }
        } else {
            castle = false;
        }

        if (castle) {
            if (isWhite)
                r[CurrentX+2, j] = true;
            else
                r[CurrentX-2, j] = true;
        }

        //Castling Left
        i = CurrentX;
        j = CurrentY;
        castle = true;
        if ((e[0] && isWhite) || (e[1] && !isWhite))
        {
            for (int k = 0; k < 3; k++) {
                c = BoardManager.Instance.Chessmans[i, j];
                if ((c == null || k == 0) && i < 7 && i > 0) {
                    if (isWhite) {
                        if (e[3]) {
                            i--;
                        } else {
                            castle = false;
                            break;
                        }
                    } else {
                        if (e[5]) {
                            i++;
                        } else {
                            castle = false;
                            break;
                        }
                    }

                } else {
                    castle = false;
                    break;
                }      

            }
        } else {
            castle = false;
        }

        if (castle) {
            if (isWhite)
                r[CurrentX-2, j] = true;
            else
                r[CurrentX+2, j] = true;
        }


        //Top Side
        i = CurrentX - 1;
        j = CurrentY + 1;
        if (CurrentY != 7)
        {
            for (int k = 0; k < 3; k++) {
                if (i >= 0 || i < 8) {
                    c = BoardManager.Instance.Chessmans[i, j];
                    if (c == null)
                        r[i, j] = true;
                    else if (isWhite != c.isWhite)
                        r[i, j] = true;
                }
                i++;
            }
        }

        //Down Side
        i = CurrentX - 1;
        j = CurrentY - 1;
        if (CurrentY != 0)
        {
            for (int k = 0; k < 3; k++) {
                if (i >= 0 || i < 8) {
                    c = BoardManager.Instance.Chessmans[i, j];
                    if (c == null)
                        r[i, j] = true;
                    else if (isWhite != c.isWhite)
                        r[i,j] = true;
                }
                i++;
            }
        }

        //Middle Left
        if (CurrentX != 0) {
            c = BoardManager.Instance.Chessmans[CurrentX - 1, CurrentY];
            if (c == null)
                r[CurrentX - 1, CurrentY]= true;
            else if (isWhite != c.isWhite)
                r[CurrentX - 1, CurrentY] = true;
        }

        //Middle Right
        if (CurrentX != 7) {
            c = BoardManager.Instance.Chessmans[CurrentX + 1, CurrentY];
            if (c == null)
                r[CurrentX + 1, CurrentY] = true;
            else if (isWhite != c.isWhite)
                r[CurrentX + 1, CurrentY] = true;
        }

        return r;

    }
}
