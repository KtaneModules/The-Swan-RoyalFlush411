using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using theSwan;

public class theSwanScript : MonoBehaviour
{
		//Bomb info
		public KMBombInfo Bomb;
		public KMAudio Audio;
		KMBombModule BombModule;

		//Buttons
		public KMSelectable execute;
		public KMSelectable button0;
		public KMSelectable button1;
		public KMSelectable button2;
		public KMSelectable button3;
		public KMSelectable button4;
		public KMSelectable button5;
		public KMSelectable button6;
		public KMSelectable button7;
		public KMSelectable button8;
		public KMSelectable button9;
		public KMSelectable button10;
		public KMSelectable button11;

		//Button Labels
		public List<TextMesh> masterLabels;
		public List<string> solveLabelOptions;
		public List<string> labelOptions;
		public List<string> decoyLabelOptions;
		public List<TextMesh> labels;
		int upperRange = 6;
		int decoyRange = 12;
		int solveRange = 12;
		int buttonIndex = 12;
		int selectionIndex;
		int pickedButton;

		//Coroutines
		Coroutine timerCoroutine;
		Coroutine shuffleCoroutine;
		Coroutine alarmCoroutine;
		Coroutine wordsCoroutine;

		//Timer
		public TextMesh digit1;
		public TextMesh digit2;
		public TextMesh digit3;
		public TextMesh digit4;
		public Renderer digit1Rend;
		public Renderer digit2Rend;
		public Renderer digit3Rend;
		public Renderer digit4Rend;
		public Renderer digit1GlyphRend;
		public Renderer digit2GlyphRend;
		public Renderer digit3GlyphRend;
		public Renderer digit4GlyphRend;
		private float digit2Time = 1;
		private float digit3Time = 10;
		private float digit4Time = 8;
		private float increment = 1f;

		//Computer screen
		public Renderer cursor;
		String computerText;
		public TextMesh cursorText;

		//Bools etc.
		bool beepReady = true;
		int resetCounter = 1;
		int timeUpCounter = 0;
		bool cursorBool = true;
		bool shuffleComplete = false;
		bool keyboardLock = true;
		bool executeLock = false;
		bool readyToSolve = false;
		decimal allModuleCount;
		decimal allSolvedModules;
		decimal modulePercentage;
		float timeRemaining;
		int chanceCalculator;
		bool timeOut = false;
		int tracker = 0;
		bool solveLogged = false;
		int systemResetCounter = 0;
		bool solved = false;

		//Logging
		static int moduleIdCounter = 1;
		int moduleId;

		//Set-up
		void Awake ()
		{
				moduleId = moduleIdCounter++;
				GetComponent<KMBombModule>().OnActivate += OnActivate;
				execute.OnInteract += delegate () { onExecute(); return false; };
				button0.OnInteract += delegate () { keyPress(button0); return false; };
        button1.OnInteract += delegate () { keyPress(button1); return false; };
        button2.OnInteract += delegate () { keyPress(button2); return false; };
        button3.OnInteract += delegate () { keyPress(button3); return false; };
        button4.OnInteract += delegate () { keyPress(button4); return false; };
        button5.OnInteract += delegate () { keyPress(button5); return false; };
        button6.OnInteract += delegate () { keyPress(button6); return false; };
        button7.OnInteract += delegate () { keyPress(button7); return false; };
        button8.OnInteract += delegate () { keyPress(button8); return false; };
        button9.OnInteract += delegate () { keyPress(button9); return false; };
        button10.OnInteract += delegate () { keyPress(button10); return false; };
        button11.OnInteract += delegate () { keyPress(button11); return false; };
		}

		void Start()
		{
				allModuleCount = Bomb.GetModuleNames().Count;
				if (allModuleCount == 0)
				{
						allModuleCount = 1;
				}
				StartCoroutine(cursorBlink());
				labelSelectorMain();
				computerText = ">: ";
				digit1GlyphRend.enabled = false;
				digit2GlyphRend.enabled = false;
				digit3GlyphRend.enabled = false;
				digit4GlyphRend.enabled = false;
		}

		void OnActivate()
		{
				timerCoroutine = StartCoroutine(timer());
		}

		void Update()
		{
				allSolvedModules = Bomb.GetSolvedModuleNames().Count;
				if (beepReady == true && digit3Time == 4 && digit4Time == 2)
				{
				alarmCoroutine = StartCoroutine(alarmChecker());
				beepReady = false;
				}

				if (keyboardLock == true && digit3Time == 4 && digit4Time == 0)
				{
						Debug.LogFormat("[The Swan #{0}] Keyboard unlocked!", moduleId);
						keyboardLock = false;
				}
				timeRemaining = Bomb.GetTime();

				if (timeRemaining < 180 && timeOut == false && digit3Time > 4 && digit3Time < 10 && readyToSolve == false)
				{
						readyToSolve = true;
						Debug.LogFormat("[The Swan #{0}] Ready to solve!", moduleId);
						timeOut = true;
						systemReset();
				}

				if (allModuleCount - allSolvedModules < 4 && readyToSolve == false)
				{
						readyToSolve = true;
						Debug.LogFormat("[The Swan #{0}] Ready to solve!", moduleId);
				}
		}

		//Button label selection
		void labelSelectorMain()
		{
				while (upperRange > 0)
				{
						labelSelectorTrue();
				}
		}

		void labelSelectorTrue()
		{
				pickedButton = UnityEngine.Random.Range(0,buttonIndex);
				selectionIndex = UnityEngine.Random.Range(0,upperRange);
				labels[pickedButton].text = labelOptions[selectionIndex];
				labels.RemoveAt(pickedButton);
				labelOptions.RemoveAt(selectionIndex);
				buttonIndex -= 1;
				upperRange -= 1;
				if (upperRange == 0)
				{
						while (buttonIndex > 0)
						{
								labelSelectorDecoy();
						}
				}
		}

		void labelSelectorDecoy()
		{
				pickedButton = UnityEngine.Random.Range(0,buttonIndex);
				selectionIndex = UnityEngine.Random.Range(0,decoyRange);
				labels[pickedButton].text = decoyLabelOptions[selectionIndex];
				labels.RemoveAt(pickedButton);
				decoyLabelOptions.RemoveAt(selectionIndex);
				buttonIndex -= 1;
				decoyRange -= 1;
		}

		void labelSelectorSolve()
		{
				while (solveRange > 0)
				{
						pickedButton = UnityEngine.Random.Range(0,buttonIndex);
						selectionIndex = UnityEngine.Random.Range(0,solveRange);
						labels[pickedButton].text = solveLabelOptions[selectionIndex];
						labels.RemoveAt(pickedButton);
						solveLabelOptions.RemoveAt(selectionIndex);
						buttonIndex -= 1;
						solveRange -= 1;
				}
		}

		//Timer
		IEnumerator timer()
		{
				while (digit3Time > 0)
				{
						while (digit4Time > 0)
						{
								yield return new WaitForSeconds(increment);
								Audio.PlaySoundAtTransform("tick", transform);
								digit4Time -= increment;
								digit4.text = digit4Time.ToString("n0");
						}
						if (digit4Time == 0)
						{
								yield return new WaitForSeconds(increment);
								Audio.PlaySoundAtTransform("tick", transform);
								digit4Time = 9;
								digit4.text = digit4Time.ToString("n0");
								if (digit3Time > 0)
								{
										digit3Time -= increment;
										digit3.text = digit3Time.ToString("n0");
								}
								if (digit2Time > 0)
								{
										digit2Time -= increment;
										digit2.text = digit2Time.ToString("n0");
								}
						}
				}
				while (digit4Time > 0)
				{
						yield return new WaitForSeconds(increment);
						if (solved == false)
						{
								Audio.PlaySoundAtTransform("tick", transform);
						}
						digit4Time -= increment;
						if (solved == false)
						digit4.text = digit4Time.ToString("n0");
				}
				if (digit4Time == 0)
				{
						executeLock = true;
						keyboardLock = true;
						if (solved == false)
						{
								yield return new WaitForSeconds(1f);
								Audio.PlaySoundAtTransform("systemFailure", transform);
						}
						else if (solved == true)
						{
								yield return new WaitForSeconds(1f);
								Audio.PlaySoundAtTransform("failsafe", transform);
						}
						if (solved == false)
						{
								while (timeUpCounter < 7)
								{
										StartCoroutine(systemShuffle1());
										StartCoroutine(systemShuffle2());
										StartCoroutine(systemShuffle3());
										StartCoroutine(systemShuffle4());
										StartCoroutine(systemShuffleDone());
										wordsCoroutine = StartCoroutine(systemFailureWords());
										yield return new WaitUntil(() => shuffleComplete == true);
										timeUpCounter ++;
										shuffleComplete = false;
								}
								yield return new WaitUntil(() => timeUpCounter > 6);
								Debug.LogFormat("[The Swan #{0}] Strike! System Failure!", moduleId);
								GetComponent<KMBombModule>().HandleStrike();
								systemReset();

						}
						else if (solved == true)
						{
								while (timeUpCounter < 9)
								{
										StartCoroutine(systemShuffle1());
										StartCoroutine(systemShuffle2());
										StartCoroutine(systemShuffle3());
										StartCoroutine(systemShuffle4());
										StartCoroutine(systemShuffleFinal());
										yield return new WaitUntil(() => shuffleComplete == true);
										timeUpCounter ++;
										shuffleComplete = false;
								}
								yield return new WaitUntil(() => timeUpCounter > 8);
								Debug.LogFormat("[The Swan #{0}] System disarmed. Module disarmed. Namaste.", moduleId);
								GetComponent<KMBombModule>().HandlePass();
								masterLabels[0].text = "D";
								masterLabels[1].text = "H";
								masterLabels[2].text = "A";
								masterLabels[3].text = "R";
								masterLabels[4].text = "M";
								masterLabels[5].text = "A";
								masterLabels[6].text = "3";
								masterLabels[7].text = ":";
								masterLabels[8].text = "S";
								masterLabels[9].text = "W";
								masterLabels[10].text = "A";
								masterLabels[11].text = "N";
								computerText = ">: ";
								cursorText.text = computerText + "▐";
						}

				}
		}

		IEnumerator systemFailureWords()
		{
				yield return new WaitForSeconds(0.1f);
				computerText = ">: S";
				cursorText.text = computerText + "▐";
				yield return new WaitForSeconds(0.1f);
				computerText = ">: Sy";
				cursorText.text = computerText + "▐";
				yield return new WaitForSeconds(0.1f);
				computerText = ">: Sys";
				cursorText.text = computerText + "▐";
				yield return new WaitForSeconds(0.1f);
				computerText = ">: Syst";
				cursorText.text = computerText + "▐";
				yield return new WaitForSeconds(0.1f);
				computerText = ">: Syste";
				cursorText.text = computerText + "▐";
				yield return new WaitForSeconds(0.1f);
				computerText = ">: System";
				cursorText.text = computerText + "▐";
				yield return new WaitForSeconds(0.1f);
				computerText = ">: System F";
				cursorText.text = computerText + "▐";
				yield return new WaitForSeconds(0.1f);
				computerText = ">: System Fa";
				cursorText.text = computerText + "▐";
				yield return new WaitForSeconds(0.1f);
				computerText = ">: System Fai";
				cursorText.text = computerText + "▐";
				yield return new WaitForSeconds(0.1f);
				computerText = ">: System Fail";
				cursorText.text = computerText + "▐";
				yield return new WaitForSeconds(0.1f);
				computerText = ">: System Failu";
				cursorText.text = computerText + "▐";
				yield return new WaitForSeconds(0.1f);
				computerText = ">: System Failur";
				cursorText.text = computerText + "▐";
				yield return new WaitForSeconds(0.1f);
				computerText = ">: System Failure";
				cursorText.text = computerText + "▐";
				StopCoroutine(wordsCoroutine);
		}

		//Cursor blink
		IEnumerator cursorBlink()
    {
		    while (cursorBool)
		    {
				    yield return new WaitForSeconds(1.2f);
				    changeCaret();
				    yield return new WaitForSeconds(0.8f);
				    changeCaret();
		    }
		}

		void changeCaret()
		{
    		if (cursorText.text.EndsWith("▐"))
    		{
        		cursorText.text = cursorText.text.Substring(0, cursorText.text.Length - 1);
    		}
    		else
    		{
        		cursorText.text = cursorText.text + "▐";
     		}
	 	}

		//Alarm checker
		IEnumerator alarmChecker()
		{
				while (digit3Time == 4 && digit4Time < 3 || digit3Time == 3 || digit3Time == 2 && digit4Time > 2)
				{
						yield return new WaitForSeconds(1.8f);
						Audio.PlaySoundAtTransform("beep", transform);
				}
				while (digit3Time == 2 && digit4Time < 3 || digit3Time == 1 && digit4Time > 1)
				{
						yield return new WaitForSeconds(1.8f);
						Audio.PlaySoundAtTransform("alarm", transform);
				}
				while (digit3Time == 1 && digit4Time < 2 || digit3Time == 0 && digit4Time > 0)
				{
						yield return new WaitForSeconds(0.9f);
						Audio.PlaySoundAtTransform("alarm", transform);
				}
		}

		//System Failure shuffles
		IEnumerator systemShuffle1()
		{
				while (resetCounter < 24 && timeUpCounter < 2)
				{
						yield return new WaitForSeconds(0.05f);
						int digit1Shuffle = UnityEngine.Random.Range(0,10);
						digit1.text = digit1Shuffle.ToString();
				}
				if (timeUpCounter == 2)
				{
						digit1Rend.enabled = false;
						digit1GlyphRend.enabled = true;
						digit1.text = "";
						Audio.PlaySoundAtTransform("glyphClick", transform);
				}
		}
		IEnumerator systemShuffle2()
		{
				while (resetCounter < 24 && timeUpCounter < 4)
				{
						yield return new WaitForSeconds(0.05f);
						int digit2Shuffle = UnityEngine.Random.Range(0,10);
						digit2.text = digit2Shuffle.ToString();
				}
				if (timeUpCounter == 4)
				{
						digit2Rend.enabled = false;
						digit2GlyphRend.enabled = true;
						digit2.text = "";
						Audio.PlaySoundAtTransform("glyphClick", transform);
				}
		}
		IEnumerator systemShuffle3()
		{
				while (resetCounter < 24 && timeUpCounter < 3)
				{
						yield return new WaitForSeconds(0.05f);
						int digit3Shuffle = UnityEngine.Random.Range(0,10);
						digit3.text = digit3Shuffle.ToString();
				}
				if (timeUpCounter == 3)
				{
						digit3Rend.enabled = false;
						digit3GlyphRend.enabled = true;
						digit3.text = "";
						Audio.PlaySoundAtTransform("glyphClick", transform);
				}
		}
		IEnumerator systemShuffle4()
		{
				while (resetCounter < 25 && timeUpCounter < 5)
				{
						yield return new WaitForSeconds(0.05f);
						int digit4Shuffle = UnityEngine.Random.Range(0,10);
						digit4.text = digit4Shuffle.ToString();
				}
				if (timeUpCounter == 5)
				{
						digit4Rend.enabled = false;
						digit4GlyphRend.enabled = true;
						digit4.text = "";
						Audio.PlaySoundAtTransform("glyphClick", transform);
				}
		}
		IEnumerator systemShuffleDone()
		{
				while (resetCounter < 25 && timeUpCounter >= 0 && timeUpCounter < 7)
				{
						yield return new WaitForSeconds(0.05f);
						upperRange = 6;
						buttonIndex = 12;
						compileLists();
						labelSelectorMain();
						resetCounter++;
				}
				while (resetCounter == 25)
				{
						yield return new WaitForSeconds(0.05f);
						shuffleComplete = true;
						resetCounter = 1;
				}
		}
		IEnumerator systemShuffleFinal()
		{
				while (resetCounter < 25 && timeUpCounter >= 0 && timeUpCounter < 9)
				{
						yield return new WaitForSeconds(0.05f);
						upperRange = 6;
						buttonIndex = 12;
						compileLists();
						labelSelectorMain();
						resetCounter++;
				}
				while (resetCounter == 25)
				{
						yield return new WaitForSeconds(0.05f);
						shuffleComplete = true;
						resetCounter = 1;
				}
		}


		//Regular shuffles
		IEnumerator shuffle1()
		{
				while (resetCounter < 24)
				{
						yield return new WaitForSeconds(0.05f);
						int digit1Shuffle = UnityEngine.Random.Range(0,10);
						digit1.text = digit1Shuffle.ToString();
				}
		}
		IEnumerator shuffle2()
		{
				while (resetCounter < 24)
				{
						yield return new WaitForSeconds(0.05f);
						int digit2Shuffle = UnityEngine.Random.Range(0,10);
						digit2.text = digit2Shuffle.ToString();
				}
		}
		IEnumerator shuffle3()
		{
				while (resetCounter < 24)
				{
						yield return new WaitForSeconds(0.05f);
						int digit3Shuffle = UnityEngine.Random.Range(0,10);
						digit3.text = digit3Shuffle.ToString();
				}
		}
		IEnumerator shuffle4()
		{
				while (resetCounter < 25)
				{
						yield return new WaitForSeconds(0.05f);
						int digit4Shuffle = UnityEngine.Random.Range(0,10);
						digit4.text = digit4Shuffle.ToString();
						upperRange = 6;
						buttonIndex = 12;
						compileLists();
						labelSelectorMain();
						resetCounter++;
				}
				while (resetCounter == 25)
				{
						shuffleComplete = true;
						resetCounter = 1;
				}
		}

		//System reset
		void systemReset()
		{
				chanceCalculator = UnityEngine.Random.Range(0,3);
				decimal modulePercentage = (allSolvedModules / allModuleCount) * 100;
				if (modulePercentage >= 40 && chanceCalculator == 0 || allModuleCount < 4 && chanceCalculator == 0)
				{
						readyToSolve = true;
						if (solveLogged == false)
						{
								Debug.LogFormat("[The Swan #{0}] Ready to solve!", moduleId);
								solveLogged = true;
						}
				}
				else if (modulePercentage >= 40 || allModuleCount < 4)
				{
						tracker++;
						if (tracker == 4)
						{
								readyToSolve = true;
								if (solveLogged == false)
								{
										Debug.LogFormat("[The Swan #{0}] Ready to solve!", moduleId);
										solveLogged = true;
								}
						}
				}
				Debug.LogFormat("[The Swan #{0}] The system has been reset {1} time(s).", moduleId, systemResetCounter);
				if (readyToSolve == true)
				{
						answerLog();
				}
				else
				{

				}
				StopCoroutine(timerCoroutine);
				if(alarmCoroutine != null) StopCoroutine(alarmCoroutine);
				digit1Rend.enabled = true;
				digit2Rend.enabled = true;
				digit3Rend.enabled = true;
				digit4Rend.enabled = true;
				digit1GlyphRend.enabled = false;
				digit2GlyphRend.enabled = false;
				digit3GlyphRend.enabled = false;
				digit4GlyphRend.enabled = false;
				timeUpCounter = 0;
				keyboardLock = true;
				executeLock = false;
				Audio.PlaySoundAtTransform("reset", transform);
				StartCoroutine(shuffle1());
				StartCoroutine(shuffle2());
				StartCoroutine(shuffle3());
				StartCoroutine(shuffle4());
				StartCoroutine(resetTimer());
		}

		IEnumerator resetTimer()
		{
				yield return new WaitUntil(() => shuffleComplete == true);
				computerText = ">: ";
				cursorText.text = computerText + "▐";
				digit1.text = "0";
				digit2.text = "1";
				digit2Time = 1;
				digit3.text = "0";
				digit3Time = 10;
				digit4.text = "8";
				digit4Time = 8;
				beepReady = true;
				shuffleComplete = false;
				compileLists();
				if (readyToSolve == true)
				{
						labelSelectorSolve();
				}
				else
				{
						labelSelectorMain();
				}
				timerCoroutine = StartCoroutine(timer());
		}

		void compileLists()
		{
				labelOptions.Clear();
				decoyLabelOptions.Clear();
				solveLabelOptions.Clear();

				buttonIndex = 12;
				upperRange = 6;
				decoyRange = 12;
				solveRange = 12;

				labelOptions.Add("4");
				labelOptions.Add("8");
				labelOptions.Add("15");
				labelOptions.Add("16");
				labelOptions.Add("23");
				labelOptions.Add("42");
				decoyLabelOptions.Add("7");
				decoyLabelOptions.Add("32");
				decoyLabelOptions.Add("18");
				decoyLabelOptions.Add("21");
				decoyLabelOptions.Add("98");
				decoyLabelOptions.Add("5");
				decoyLabelOptions.Add("24");
				decoyLabelOptions.Add("32");
				decoyLabelOptions.Add("27");
				decoyLabelOptions.Add("52");
				decoyLabelOptions.Add("48");
				decoyLabelOptions.Add("46");
				labels.Add(masterLabels[0]);
				labels.Add(masterLabels[1]);
				labels.Add(masterLabels[2]);
				labels.Add(masterLabels[3]);
				labels.Add(masterLabels[4]);
				labels.Add(masterLabels[5]);
				labels.Add(masterLabels[6]);
				labels.Add(masterLabels[7]);
				labels.Add(masterLabels[8]);
				labels.Add(masterLabels[9]);
				labels.Add(masterLabels[10]);
				labels.Add(masterLabels[11]);
				solveLabelOptions.Add("D");
				solveLabelOptions.Add("H");
				solveLabelOptions.Add("A");
				solveLabelOptions.Add("R");
				solveLabelOptions.Add("M");
				solveLabelOptions.Add("7");
				solveLabelOptions.Add("S");
				solveLabelOptions.Add("W");
				solveLabelOptions.Add("N");
				solveLabelOptions.Add("H");
				solveLabelOptions.Add("T");
				solveLabelOptions.Add("C");
		}

		//Failsafe
		IEnumerator failsafe()
		{
				Debug.LogFormat("[The Swan #{0}] Failsafe sequence initiated.", moduleId);
				solved = true;
				keyboardLock = true;
				yield return new WaitForSeconds(0.05f);
				StopCoroutine(timerCoroutine);
                if(alarmCoroutine != null) StopCoroutine(alarmCoroutine);
				digit1.text = "0";
				digit2.text = "0";
				digit3.text = "0";
				digit4.text = "0";
				digit2Time = 0;
				digit3Time = 0;
				digit4Time = 1;
				timerCoroutine = StartCoroutine(timer());
		}

		//Buttons
		public void keyPress(KMSelectable buttonName)
		{
				Audio.PlaySoundAtTransform("keyPress", transform);
				if (computerText.Length < 19)
				{
						if (keyboardLock == true)
						{

						}
						else
						{
								if (buttonName == button0)
								{
										computerText += masterLabels[0].text;
										cursorText.text = computerText + "▐";
										if (masterLabels[0].text != "7")
										{
												computerText += " ";
										}
								}
								else if (buttonName == button1)
								{
									computerText += masterLabels[1].text;
									cursorText.text = computerText + "▐";
									if (masterLabels[1].text != "7")
									{
											computerText += " ";
									}
								}
								else if (buttonName == button2)
								{
									computerText += masterLabels[2].text;
									cursorText.text = computerText + "▐";
									if (masterLabels[2].text != "7")
									{
											computerText += " ";
									}
								}
								else if (buttonName == button3)
								{
									computerText += masterLabels[3].text;
									cursorText.text = computerText + "▐";
									if (masterLabels[3].text != "7")
									{
											computerText += " ";
									}
								}
								else if (buttonName == button4)
								{
									computerText += masterLabels[4].text;
									cursorText.text = computerText + "▐";
									if (masterLabels[4].text != "7")
									{
											computerText += " ";
									}
								}
								else if (buttonName == button5)
								{
									computerText += masterLabels[5].text;
									cursorText.text = computerText + "▐";
									if (masterLabels[5].text != "7")
									{
											computerText += " ";
									}
								}
								else if (buttonName == button6)
								{
									computerText += masterLabels[6].text;
									cursorText.text = computerText + "▐";
									if (masterLabels[6].text != "7")
									{
											computerText += " ";
									}
								}
								else if (buttonName == button7)
								{
									computerText += masterLabels[7].text;
									cursorText.text = computerText + "▐";
									if (masterLabels[7].text != "7")
									{
											computerText += " ";
									}
								}
								else if (buttonName == button8)
								{
									computerText += masterLabels[8].text;
									cursorText.text = computerText + "▐";
									if (masterLabels[8].text != "7")
									{
											computerText += " ";
									}
								}
								else if (buttonName == button9)
								{
									computerText += masterLabels[9].text;
									cursorText.text = computerText + "▐";
									if (masterLabels[9].text != "7")
									{
											computerText += " ";
									}
								}
								else if (buttonName == button10)
								{
									computerText += masterLabels[10].text;
									cursorText.text = computerText + "▐";
									if (masterLabels[10].text != "7")
									{
											computerText += " ";
									}
								}
								else if (buttonName == button11)
								{
									computerText += masterLabels[11].text;
									cursorText.text = computerText + "▐";
									if (masterLabels[11].text != "7")
									{
											computerText += " ";
									}
								}
						}
				}
		}

		public void onExecute()
		{
				Audio.PlaySoundAtTransform("keyPress", transform);
				if (executeLock == true)
				{

				}
				else if (keyboardLock == true)
				{
						Debug.LogFormat("[The Swan #{0}] Strike! The alarm has not yet sounded.", moduleId);
						GetComponent<KMBombModule>().HandleStrike();
				}
				else if (computerText == ">: 4 8 15 16 23 42 ")
				{
						systemResetCounter++;
						systemReset();
				}
				else if (computerText == ">: D H A R M A " && new[] {8, 10, 14, 16, 24}.Contains(systemResetCounter))
				{
						StartCoroutine(failsafe());
				}
				else if (computerText == ">: H A T C H " && new[] {1, 2, 5, 11, 18, 20}.Contains(systemResetCounter))
				{
						StartCoroutine(failsafe());
				}
				else if (computerText == ">: S W N " && new[] {3, 9}.Contains(systemResetCounter))
				{
						StartCoroutine(failsafe());
				}
				else if (computerText == ">: D A R M A " && new[] {7, 13}.Contains(systemResetCounter))
				{
						StartCoroutine(failsafe());
				}
				else if (computerText == ">: S W A N " && new[] {0, 4, 6, 12, 17, 23}.Contains(systemResetCounter))
				{
						StartCoroutine(failsafe());
				}
				else if (computerText == ">: H T C H " && new[] {15, 19, 21, 22}.Contains(systemResetCounter))
				{
						StartCoroutine(failsafe());
				}
				else if (computerText == ">: 77" && systemResetCounter >= 25)
				{
						StartCoroutine(failsafe());
				}
				else
				{
						Debug.LogFormat("[The Swan #{0}] Strike! Wrong code! You entered {1}.", moduleId, computerText);
						GetComponent<KMBombModule>().HandleStrike();
						computerText = ">: ";
						cursorText.text = computerText + "▐";
				}
		}

		void answerLog()
		{
				if (new[] {8, 10, 14, 16, 24}.Contains(systemResetCounter))
				{
						Debug.LogFormat("[The Swan #{0}] Rule 1 applies. Enter '>: D H A R M A '.", moduleId);
				}
				else if (new[] {1, 2, 5, 11, 18, 20}.Contains(systemResetCounter))
				{
						Debug.LogFormat("[The Swan #{0}] Rule 2 applies. Enter '>: H A T C H '.", moduleId);
				}
				else if (new[] {3, 9}.Contains(systemResetCounter))
				{
						Debug.LogFormat("[The Swan #{0}] Rule 3 applies. Enter '>: S W N '.", moduleId);
				}
				else if (new[] {7, 13}.Contains(systemResetCounter))
				{
						Debug.LogFormat("[The Swan #{0}] Rule 4 applies. Enter '>: D A R M A '.", moduleId);
				}
				else if (new[] {0, 4, 6, 12, 17, 23}.Contains(systemResetCounter))
				{
						Debug.LogFormat("[The Swan #{0}] Rule 5 applies. Enter '>: S W A N '.", moduleId);
				}
				else if (new[] {15, 22}.Contains(systemResetCounter))
				{
						Debug.LogFormat("[The Swan #{0}] Rules 3 & 4 and Exception 1 apply. Enter '>: H T C H '.", moduleId);
				}
				else if (new[] {19, 21}.Contains(systemResetCounter))
				{
						Debug.LogFormat("[The Swan #{0}] Rules 1 & 3 and Exception 1 apply. Enter '>: H T C H '.", moduleId);
				}
				else
				{
						Debug.LogFormat("[The Swan #{0}] Exception 2 applies. Enter 77.", moduleId);
				}
		}

    void TwitchHandleForcedSolve()
    {
        //For cases where Twitch plays admins do !<id> solve on the module.
        if (solved) return;
        systemResetCounter = -1;  //Just in case souvenir asks how many resets in the future. Set to zero for this case.
        StartCoroutine(failsafe());
    }

    private string TwitchHelpMessage = "Execute the command with !{0} execute 5 12 2 7 9 4. Get the actual time remaining with !{0} time. (Buttons are in reading order from 1-6 top row, 7-12 bottom row.)";
    private IEnumerator ProcessTwitchCommand(string command)
    {
        List<KMSelectable> buttons = new List<KMSelectable>();
        string[] split = command.ToLowerInvariant().Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
        if (Regex.IsMatch(command,"^(submit|execute) [0-9 ]+$",RegexOptions.CultureInvariant | RegexOptions.IgnoreCase))
        {
            int i;
            if (split.Skip(1).Any(x => !int.TryParse(x, out i) || i < 1 || i > 12)) yield break;
            foreach (int input in split.Skip(1).Select(int.Parse))
            {
                switch (input)
                {
                    case 1: buttons.Add(button0); break;
                    case 2: buttons.Add(button1); break;
                    case 3: buttons.Add(button2); break;
                    case 4: buttons.Add(button3); break;
                    case 5: buttons.Add(button4); break;
                    case 6: buttons.Add(button5); break;
                    case 7: buttons.Add(button6); break;
                    case 8: buttons.Add(button7); break;
                    case 9: buttons.Add(button8); break;
                    case 10: buttons.Add(button9); break;
                    case 11: buttons.Add(button10); break;
                    case 12: buttons.Add(button11); break;
                    default: yield break;
                }
            }
            yield return null;
            foreach (KMSelectable button in buttons)
            {
                button.OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            execute.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        else if (split[0].Equals("time"))
        {
            yield return null;
            yield return string.Format("sendtochat The time remaining is {0}{1} seconds.", digit3Time, digit4Time);
        }
    }
}
