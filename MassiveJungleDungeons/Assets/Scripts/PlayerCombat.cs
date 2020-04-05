﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : UnitCombat
{
    private PlayerMove _playerMove;

    private float _buttonStartTime; // when right-mouse is pressed
    private float _buttonTimePressed; // how long right-mouse was held

    private void Awake()
    {
        _playerMove = this.GetComponent<PlayerMove>();
        previousHealth = health;
    }

    private void Start()
    {
        Init();
    }

    public void Reset()
    {
        if(_targetTile != null)
        {
            _targetTile.SetActiveSelectors(false, false, false);
            _targetTile = null;

            _target = null;
        }

        if(_currentTile != null)
        {
            _currentTile.SetActiveSelectors(false, false, false);
            _currentTile = null;
        }

        _tilesInRange.Clear();
    }

    private void Update()
    {
        DrawHealthBar();

        if (_teamID != GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().GetActiveTeamID())
        {
            if (healthBar.gameObject.activeInHierarchy && !_displayForCombatSelection)
                healthBar.gameObject.SetActive(false);
            state = CombatState.Idle;
            return;
        }

        if (_playerMove.state != UnitMove.MoveState.Idle && _playerMove.state != UnitMove.MoveState.Moved)
        {
            state = CombatState.Idle;
            return;
        }

        switch (state)
        {
            default:
            case CombatState.Idle:
                if (Input.GetMouseButtonDown(1))
                {
                    if (!Input.GetMouseButtonUp(1))
                        _buttonStartTime = Time.time;

                    FindTilesInRange();

                    _target = GetTarget();
                    if(_target != null)
                        if (Physics.Raycast(_target.transform.position, Vector3.down, out var hit, 1))
                            if (hit.collider.tag == "Tile")
                                _targetTile = hit.collider.gameObject.GetComponent<Tile>();

                    SetUnitUIs(true);

                    state = CombatState.Selected;
                }
                break;

            case CombatState.Selected:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    RemoveSelectedTiles();

                    if (_target == null)
                    {
                        SetUnitUIs(false);
                        state = CombatState.Idle;
                    }
                    else
                    {
                        _targetTile.SetActiveSelectors(false, true, false);
                        state = CombatState.Attacking;
                    }
                }

                if (!Input.GetMouseButtonDown(1) && Input.GetMouseButtonUp(1))
                {
                    _buttonTimePressed = Time.time - _buttonStartTime;
                    if (_buttonTimePressed > 0.3f)
                    {
                        RemoveSelectedTiles();
                        SetUnitUIs(false);
                        state = CombatState.Idle;
                    }
                }

                else if (Input.GetMouseButtonDown(1))
                {
                    RemoveSelectedTiles();
                    SetUnitUIs(false);
                    state = CombatState.Idle;
                }
                break;

            case CombatState.Attacking:
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    if (_targetTile != null)
                    {
                        _targetTile.SetActiveSelectors(false, true, false);
                        _targetTile.Reset(false, true);
                    }

                    switch (this.GetComponent<PlayerState>().GetElementalState())
                    {
                        default:
                        case UnitState.ElementalState.Grass:
                            DealDamage(40);
                            break;

                        case UnitState.ElementalState.Water:
                            DealDamage(30);
                            break;

                        case UnitState.ElementalState.Fire:
                            DealDamage(50);
                            break;
                    }
                    state = CombatState.Attacked;
                }
                break;

            case CombatState.Attacked:
                if(_targetTile != null)
                {
                    _targetTile.SetActiveSelectors(false, false, false);
                    _targetTile = null;
                    _target = null;

                    StartCoroutine(SetUnitUIs(false, 1.5f));
                }
                break;

            case CombatState.Dead:
                break;
        }
    }

    private void DealDamage(int amount)
    {
        if (_target != null)
        {
            var target = _target.GetComponent<PlayerCombat>();
            target.previousHealth = target.health;
            target.health -= amount;
            if (target.health <= 0)
                target.state = CombatState.Dead;
        }
    }

    private void DrawHealthBar()
    {
        if (!healthBar.gameObject.activeInHierarchy)
            healthBar.gameObject.SetActive(true);

        if(previousHealth != health)
            previousHealth -= 1;
        healthFill.value = (float) previousHealth / maxHealth;

        var currentPosition = transform.position;
        healthBar.position = new Vector3(currentPosition.x + _healthBarXOffset, currentPosition.y + _healthBarYOffset, currentPosition.z);

        healthBar.LookAt(new Vector3(healthBarRotation.transform.position.x, Camera.main.transform.position.y, healthBarRotation.transform.position.z));
    }

    IEnumerator SetUnitUIs(bool shouldBeActive, float timeDelay)
    {
        yield return new WaitForSeconds(timeDelay);
        SetUnitUIs(shouldBeActive);
    }

    public void SetUnitUIs(bool shouldBeActive)
    {
        foreach (var unit in GameObject.FindGameObjectsWithTag("Unit"))
        {
            var unitCombat = unit.GetComponent<PlayerCombat>();

            if (unitCombat.GetTeamID() == GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().GetActiveTeamID()) continue;

            unitCombat.SetDisplayForCombatSelection(shouldBeActive);
            unitCombat.healthBar.gameObject.SetActive(shouldBeActive);

            var unitState = unit.GetComponent<PlayerState>();
            unitState.SetDisplayForCombatSelection(shouldBeActive);
            unitState.elementalTriangle.gameObject.SetActive(shouldBeActive);
        }
    }
}
