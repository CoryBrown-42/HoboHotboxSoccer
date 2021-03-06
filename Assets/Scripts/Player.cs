﻿/*********
 *		Author:  James Keeling
 *		Purpose: Provide the basic framework for the player and player movement. Also used by all drug/powerup scripts.
 ********/
 
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;

public class Player : MonoBehaviour
{
	public float speed = 10.0F;
	private Animator animator;
	public Slider sprintSlider;

	public float maxSprintTime;
	float currentSprintTime;

	public string configFileName = "player1controls.ini";	// default config file name. this will just mimic player1's input on all players

	Dictionary<string, KeyCode> controlMappings = new Dictionary<string, KeyCode>();	// hashtable for storing the generic strings describing what function happens with the keycode representing the button pressed

	// Use this for initialization
	void Start()
	{
		animator = GetComponent<Animator>();
		currentSprintTime = maxSprintTime;
		sprintSlider.maxValue = maxSprintTime;
		sprintSlider.value = currentSprintTime;
	}

	private void Awake()
	{
		GetConfigFile();	// must be here as it is not calling correctly in start
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		// Movement for characters
		Vector3 moveVector = Vector3.zero;	// movement vector to add to the player position. allows for us to move in diagonals, but not make it faster than moving only in one direction
		
		// reads from Dictionary to get correct button names
		if ( Input.GetKey( controlMappings[ "up" ] ) )
			moveVector += Vector3.up;
		if ( Input.GetKey( controlMappings[ "down" ] ) )
			moveVector += Vector3.down;
		if ( Input.GetKey( controlMappings[ "left" ] ) )
			moveVector += Vector3.left;
		if ( Input.GetKey( controlMappings[ "right" ] ) )
			moveVector += Vector3.right;
		
		if ( moveVector == Vector3.zero )	// no movement happeneing, reset animator bool
		{
			animator.SetBool( "isWalking", false );
			RefillSprint();
		}
		else
		{
			moveVector.Normalize(); // set the value of the direction moving to 1 unit
			if ( currentSprintTime > 0 && Input.GetKey( controlMappings[ "sprint" ] )  ) // figure out if we are trying to sprint
			{
				transform.Translate( moveVector * speed * 2 * Time.deltaTime );
				DrainSprint();
			}
			else
			{
				transform.Translate( moveVector * speed * Time.deltaTime );
				if ( !Input.GetKey( controlMappings[ "sprint" ] ) )
					RefillSprint();
			}
			animator.SetBool( "isWalking", true );	// make sure animator is set to moving
		}
	}

	/// <summary>
	/// Creates or Opens a file from which to read the input for a given player
	/// </summary>
	void GetConfigFile()
	{
		// opening file and reading stream
		FileStream fstream = new FileStream( configFileName, FileMode.OpenOrCreate );
		StreamReader fin = new StreamReader( fstream );

		List<string> linesFromFile = new List<string>();	// a list to hold the lines from the file as strings

		while ( !fin.EndOfStream )	// run until end of file
		{
			linesFromFile.Add( fin.ReadLine() );	// add each line to linesFromFile
		}

		// cycle through lines from the file and translate into Dictionary
		foreach ( string str in linesFromFile )
		{
			str.Trim( '\n' );	// remove new line characters
			string[] tempArr = str.Split( '=' );    // uses = as the delimiter to break string into parts
			
			// create dictionary entry for current line
			controlMappings.Add( tempArr[ 0 ], 
				(KeyCode)Enum.Parse(typeof(KeyCode), tempArr[1]) );	// this line searches the KeyCode enum to match the string value from the file to it's corresponding enum value
		}

		// close file and reading stream
		fin.Close();
		fstream.Close();
	}

	/// <summary>
	/// Updates the dictionary entry for a control
	/// </summary>
	/// <param name="controlName">Control name to be updated</param>
	/// <param name="key">New key to associate with control</param>
	public void ChangeControls( string controlName, KeyCode key )
	{
		controlMappings[ controlName ] = key;
	}

	/// <summary>
	/// Returns the current control mappings dictionary
	/// </summary>
	/// <returns></returns>
	public Dictionary<string, KeyCode> GetControlMappings()
	{
		return controlMappings;
	}

	/// <summary>
	/// Saves the control mappings dictionary to the specified config file
	/// </summary>
	public void SaveControlsToFile()
	{
		// open file and writing stream
		FileStream fstream = new FileStream( configFileName, FileMode.Truncate );
		StreamWriter fout = new StreamWriter( fstream );

		int count = 1;	// count to see if we are on the last entry
		foreach ( KeyValuePair<string, KeyCode> pair in controlMappings )	// cycle through dictionary
		{
			if ( count < controlMappings.Count )	// count is not at the end of the control mappings
			{
				fout.WriteLine( pair.Key + "=" + pair.Value.ToString() );	// write the whole line and newline character
				count++;	// increment our count
			}
			else
				fout.Write( pair.Key + "=" + pair.Value.ToString() );	// at the end of the file, don't write a newline character
		}

		// flush to disk and close file and writer streams
		fout.Flush();
		fout.Close();
		fstream.Close();
	}

	/// <summary>
	/// Drain the sprint meter by however much time has elapsed between frames
	/// </summary>
	void DrainSprint()
	{
		currentSprintTime -= Time.deltaTime;	// decrement the time between frames from the current total
		if ( currentSprintTime < 0 )	// check for negative values
			currentSprintTime = 0;
		sprintSlider.value = currentSprintTime;	// update the slider value
	}

	/// <summary>
	/// Refill the sprint meter
	/// </summary>
	void RefillSprint()
	{
		currentSprintTime += Time.deltaTime * 0.5f;	// add half the amount of time from between frames to the current total
		if ( currentSprintTime > maxSprintTime )	// check if it exceeds max amount
			currentSprintTime = maxSprintTime;
		sprintSlider.value = currentSprintTime;	// update slider
	}
}
