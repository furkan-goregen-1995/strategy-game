﻿using UnityEngine;


public class Enemy : MonoBehaviour {

	EnemyFactory originFactory;

	[SerializeField]
	Transform model = default;

	

	float speed;

	float pathOffset;

	Direction direction;
	DirectionChange directionChange;
	float directionAngleFrom, directionAngleTo;

	GameaTile tileFrom, tileTo;
	Vector3 positionFrom, positionTo;
	float progress, progressFactor;

	public EnemyFactory OriginFactory {
		get => originFactory;
		set {
			Debug.Assert(originFactory == null, "Redefined origin factory!");
			originFactory = value;
		}
	}
	
public void Initialize (float scale, float speed, float pathOffset) {
		model.localScale = new Vector3(scale, scale, scale);
		this.speed = speed;
		this.pathOffset = pathOffset;
	}


public void SpawnOn (GameaTile tile) {
		//transform.localPosition = tile.transform.localPosition;
		Debug.Assert(tile.NextTileOnPath != null, "Nowhere to go!", this);
		tileFrom = tile;
		tileTo = tile.NextTileOnPath;
		//positionFrom = tileFrom.transform.localPosition;
		//positionTo = tileFrom.ExitPoint;
		//transform.localRotation = tileFrom.PathDirection.GetRotation();
		progress = 0f;
		PrepareIntro();
		
	}

	void PrepareIntro () {
		positionFrom = tileFrom.transform.localPosition;
		positionTo = tileFrom.ExitPoint;
		direction = tileFrom.PathDirection;
		directionChange = DirectionChange.None;
		directionAngleFrom = directionAngleTo = direction.GetAngle();
		model.localPosition = new Vector3(pathOffset, 0f);
		transform.localRotation = direction.GetRotation();
		progressFactor = 2f*speed;
	}

		void PrepareOutro () {
		positionTo = tileFrom.transform.localPosition;
		directionChange = DirectionChange.None;
		directionAngleTo = direction.GetAngle();
		model.localPosition =  new Vector3(pathOffset, 0f);
		transform.localRotation = direction.GetRotation();
		progressFactor = 2f*speed;
	}

		public bool GameUpdate () {
		progress += Time.deltaTime * progressFactor;
		while (progress >= 1f) {
		//tileFrom = tileTo;
		//tileTo = tileTo.NextTileOnPath;
		if (tileTo == null) {
				OriginFactory.Reclaim(this);
				return false;
			}
		//positionFrom = positionTo;
		//positionTo = tileFrom.ExitPoint;
		//transform.localRotation = tileFrom.PathDirection.GetRotation();
		//progress -= 1f;
		progress = (progress - 1f) / progressFactor;
		PrepareNextState();
		progress *= progressFactor;
		}
		if (directionChange == DirectionChange.None) {
			transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);
		}
		//if (directionChange != DirectionChange.None) {
		else{	
			float angle = Mathf.LerpUnclamped(
				directionAngleFrom, directionAngleTo, progress
			);
			transform.localRotation = Quaternion.Euler(0f, angle, 0f);
			}	
		return true;
	}
	void PrepareNextState () {
		tileFrom = tileTo;
		tileTo = tileTo.NextTileOnPath;
		positionFrom = positionTo;
		if (tileTo == null) {
			PrepareOutro();
			return;
		}
		positionTo = tileFrom.ExitPoint;
		directionChange = direction.GetDirectionChangeTo(tileFrom.PathDirection);
		direction = tileFrom.PathDirection;
		directionAngleFrom = directionAngleTo;
		switch (directionChange) {
			case DirectionChange.None: PrepareForward(); break;
			case DirectionChange.TurnRight: PrepareTurnRight(); break;
			case DirectionChange.TurnLeft: PrepareTurnLeft(); break;
			default: PrepareTurnAround(); break;
		}
	}

		void PrepareForward () {
		transform.localRotation = direction.GetRotation();  //Transform Essentials
		directionAngleTo = direction.GetAngle();
		model.localPosition = new Vector3(pathOffset, 0f);
		progressFactor = speed;
	}

	void PrepareTurnRight () {
		directionAngleTo = directionAngleFrom + 90f;
		model.localPosition = new Vector3(pathOffset-0.5f, 0f);
		transform.localPosition = positionFrom + direction.GetHalfVector();
		progressFactor = speed / (Mathf.PI *  0.5f * (0.5f - pathOffset));
	}

	void PrepareTurnLeft () {
		directionAngleTo = directionAngleFrom - 90f;
		model.localPosition = new Vector3(pathOffset + 0.5f, 0f);
		transform.localPosition = positionFrom + direction.GetHalfVector();
		progressFactor = speed / (Mathf.PI * 0.5f * (0.5f + pathOffset));
	}

	void PrepareTurnAround () {
		directionAngleTo = directionAngleFrom + (pathOffset < 0f ? 180f : -180f);
		model.localPosition =  new Vector3(pathOffset, 0f);
		transform.localPosition = positionFrom;
		progressFactor = speed / (Mathf.PI * Mathf.Max(Mathf.Abs(pathOffset), 0.2f));
	}
}