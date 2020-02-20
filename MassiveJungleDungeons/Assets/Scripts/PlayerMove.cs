﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//==============================================================================

public class PlayerMove : UnitMove
{
    //==========================================================================

    private void CheckMouse()
    {
        if(Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                if(hit.collider.CompareTag("Tile"))
                {
                    Tile t = hit.collider.GetComponent<Tile>();
                    if(t.state == Tile.TileState.SELECTED)
                        MoveToTile(t);
                }
            }
        }
    }

    //==========================================================================

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        switch (state) // Movement states not Unit states
        {
            // View movement range
            default:
            case MoveState.IDLE:
                if (Input.GetKeyDown(KeyCode.Space))
                    state = MoveState.SELECTING;
                break;
            
            // Choosing what selectable tile to go to
            case MoveState.SELECTING:
                FindSelectableTiles();
                CheckMouse();
                break;

            // Choosing destination tile
            case MoveState.MOVING:
                Move();
                break;
        }
    }
}
