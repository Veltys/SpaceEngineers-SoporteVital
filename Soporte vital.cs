using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;


/// <file>Soporte vital.cs</file>
/// <summary>Small vital support room manager script for Space Engineers game</summary>
/// <author>Veltys</author>
/// <date>2022-05-06</date>
/// <version>1.1.0</version>
/// <note>Made just for internal use</note>


namespace ScriptingClass {

    /// <summary>
    /// Program class
    /// Provides the container for a programmable block program
    /// Needs to be called "Program" because ingame programmable block assume that
    /// </summary>
    class Program {
        // Some stuff for avoiding some environment errors
        IMyGridTerminalSystem? GridTerminalSystem = null;
        IMyGridProgramRuntimeInfo? Runtime = null;
        Action<string>? Echo = null;
        IMyTerminalBlock? Me = null;

        // Start copying to game after this text

        private const bool _log = true;                                                     // Log changes to console and programmable block screens

        private const string _scriptName = "Soporte vital mgr.";                            // Script name
        private const string _scriptVersion = "1.1.0";                                      // Script version

        private string _logText;                                                            // Log text container
        readonly private string _nameAirVent, _nameButtonPannels, _nameEngines,             // Various names (formely described)
            _nameGasGenerators, _nameLights, _nameReactors, _nameTanksH2, _nameTanksO2;

        public readonly Color colourCyan, colourGreen, colourRed, colourWhite,              // Various colours (formely described) 
            colourYellow;

        private readonly List<IMyTextSurface> _screens;                                              // Screens list


        /// <summary>
        /// Class constructor
        /// Set-up all variables and programmable block screens
        /// </summary>
        public Program() {
            ushort i;


            Runtime.UpdateFrequency = UpdateFrequency.Update100;                            // Update frequency (avoid using a Timer Block)


            _nameAirVent = "Respiradero int. soporte vital base";                           // Name of the Air vent block supervised
            _nameButtonPannels = "PBCFs soporte vital base";                                // Name of the Button pannels group managed
            _nameEngines = "Motores de H2 soporte vital base";                              // Name of the H2 engines group supervised
            _nameGasGenerators = "Generadores de O2/H2 soporte vital base";                 // Name of the H2 / O2 generators group supervised
            _nameLights = "Iluminaciones interiores estados soporte vital base";            // Name of the lights group managed
            _nameReactors = "Reactores soporte vital base";                                 // Name of the Reactors group supervised
            _nameTanksH2 = "Tanques de H2 soporte vital base";                              // Name of the H2 tanks group supervised
            _nameTanksO2 = "Tanques de O2 soporte vital base";                              // Name of the O2 tanks group supervised

            colourCyan = new Color(0, 255, 255);                                            // Cyan colour
            colourGreen = new Color(0, 255, 0);                                             // Green colour
            colourRed = new Color(255, 0, 0);                                               // Red colour
            colourWhite = new Color(255, 255, 255);                                         // White colour
            colourYellow = new Color(255, 255, 0);                                          // Yellow colour

            _screens = new List<IMyTextSurface>();                                          // Screens list of the programmable block

            for(i = 0; i < Me.SurfaceCount; i++) {                                          // Adding screens to the list
                _screens.Add(Me.GetSurface(i));
            }

            if(_log) {                                                                      // Setting-up screens
                for(i = 0; i < _screens.Count; i++) {
                    _screens[i].ContentType = ContentType.TEXT_AND_IMAGE;
                    _screens[i].Script = "";
                }

                _screens[0].Alignment = TextAlignment.LEFT;
                _screens[0].Font = "Monospace";
                _screens[0].FontSize = 0.5f;
                _screens[0].TextPadding = 2.0f;

                _screens[1].Alignment = TextAlignment.CENTER;
                _screens[1].Font = "Debug";
                _screens[1].FontSize = 3.0f;
                _screens[1].TextPadding = 25.0f;
            }
            else {
                for(i = 0; i < _screens.Count; i++) {
                    _screens[i].ContentType = ContentType.NONE;
                    _screens[i].Script = "";
                }
            }
        }


        /// <summary>
        /// Overloaded SetLight private method for air vent blocks
        /// Reads given block status and change given light to a colour that matches the block state
        /// </summary>
        /// <param name="light">Light to manage</param>
        /// <param name="block">Air vent to supervise</param>
        private void SetLight(IMyLightingBlock light, IMyAirVent block) {
            if(block.Enabled) {
                if(block.Depressurize) {
                    light.SetValue("Color", colourCyan);

                    LogLightChange(light, "cyan");
                }
                else if(!block.CanPressurize) {
                    light.SetValue("Color", colourYellow);

                    LogLightChange(light, "amarillo");
                }
                else {
                    light.SetValue("Color", colourGreen);

                    LogLightChange(light, "verde");
                }
            }
            else {
                light.SetValue("Color", colourRed);

                LogLightChange(light, "rojo");
            }
        }


        /// <summary>
        /// Overloaded SetLight private method for H2 / O2 generator blocks
        /// Reads given block status and change given light to a colour that matches the block state
        /// </summary>
        /// <param name="light">Light to manage</param>
        /// <param name="block">H2 / O2 generator to supervise</param>
        private void SetLight(IMyLightingBlock light, IMyGasGenerator block) {
            if(block.Enabled) {
                if(block.IsWorking) {
                    light.SetValue("Color", colourCyan);

                    LogLightChange(light, "cyan");
                }
                else {
                    light.SetValue("Color", colourGreen);

                    LogLightChange(light, "verde");
                }
            }
            else {
                light.SetValue("Color", colourRed);

                LogLightChange(light, "rojo");
            }
        }


        /// <summary>
        /// Overloaded SetLight private method for gas (H2 or O2) tank blocks
        /// Reads given block status and change given light to a colour that matches the block state
        /// </summary>
        /// <param name="light">Light to manage</param>
        /// <param name="block">Gas tank to supervise</param>
        private void SetLight(IMyLightingBlock light, IMyGasTank block) {
            if(block.Enabled) {
                if(block.Stockpile) {
                    light.SetValue("Color", colourCyan);

                    LogLightChange(light, "cyan");
                }
                else {
                    light.SetValue("Color", colourGreen);

                    LogLightChange(light, "verde");
                }
            }
            else {
                light.SetValue("Color", colourRed);

                LogLightChange(light, "rojo");
            }
        }


        /// <summary>
        /// Overloaded SetLight private method for other blocks
        /// Reads given block status and change given light to a colour that matches the block state
        /// </summary>
        /// <param name="light">Light to manage</param>
        /// <param name="block">Block to supervise</param>
        private void SetLight(IMyLightingBlock light, IMyFunctionalBlock block) {
            if(block.Enabled) {
                light.SetValue("Color", colourGreen);

                LogLightChange(light, "verde");
            }
            else {
                light.SetValue("Color", colourRed);

                LogLightChange(light, "rojo");
            }
        }


        /// <summary>
        /// Overloaded SetScreen private method for air vent blocks
        /// Reads given block status and change given screen text to a colour that matches the block state
        /// </summary>
        /// <param name="screen">Screen to manage</param>
        /// <param name="block">Air vent to supervise</param>
        private void SetScreen(IMyTextSurface screen, IMyAirVent block) {
            if(block.Enabled) {
                if(block.Depressurize) {
                    screen.BackgroundColor = colourCyan;
                    screen.FontColor = colourWhite;

                    LogScreenChange(screen, "cyan");
                }
                else if(!block.CanPressurize) {
                    screen.BackgroundColor = colourYellow;
                    screen.FontColor = colourWhite;

                    LogScreenChange(screen, "amarillo");
                }
                else {
                    screen.BackgroundColor = colourGreen;
                    screen.FontColor = colourWhite;

                    LogScreenChange(screen, "verde");
                }
            }
            else {
                screen.BackgroundColor = colourRed;
                screen.FontColor = colourWhite;

                LogScreenChange(screen, "rojo");
            }
        }


        /// <summary>
        /// Overloaded SetLight private method for H2 / O2 generator blocks
        /// Reads given block status and change given screen text to a colour that matches the block state
        /// </summary>
        /// <param name="screen">Screen to manage</param>
        /// <param name="block">H2 / O2 generator to supervise</param>
        private void SetScreen(IMyTextSurface screen, IMyGasGenerator block) {
            if(block.Enabled) {
                if(block.IsWorking) {
                    screen.BackgroundColor = colourCyan;
                    screen.FontColor = colourWhite;

                    LogScreenChange(screen, "cyan");
                }
                else {
                    screen.BackgroundColor = colourGreen;
                    screen.FontColor = colourWhite;

                    LogScreenChange(screen, "verde");
                }
            }
            else {
                screen.BackgroundColor = colourRed;
                screen.FontColor = colourWhite;

                LogScreenChange(screen, "rojo");
            }
        }


        /// <summary>
        /// Overloaded SetLight private method for gas (H2 or O2) tank blocks
        /// Reads given block status and change given screen text to a colour that matches the block state
        /// </summary>
        /// <param name="screen">Screen to manage</param>
        /// <param name="block">Gas tank to supervise</param>
        private void SetScreen(IMyTextSurface screen, IMyGasTank block) {
            if(block.Enabled) {
                if(block.Stockpile) {
                    screen.BackgroundColor = colourCyan;
                    screen.FontColor = colourWhite;

                    LogScreenChange(screen, "cyan");
                }
                else {
                    screen.BackgroundColor = colourGreen;
                    screen.FontColor = colourWhite;

                    LogScreenChange(screen, "verde");
                }
            }
            else {
                screen.BackgroundColor = colourRed;
                screen.FontColor = colourWhite;

                LogScreenChange(screen, "rojo");
            }
        }


        /// <summary>
        /// Overloaded SetLight private method for other blocks
        /// Reads given block status and change given screen text to a colour that matches the block state
        /// </summary>
        /// <param name="screen">Screen to manage</param>
        /// <param name="block">Gas tank to supervise</param>
        private void SetScreen(IMyTextSurface screen, IMyFunctionalBlock block) {
            if(block.Enabled) {
                screen.BackgroundColor = colourGreen;
                screen.FontColor = colourWhite;

                LogScreenChange(screen, "verde");
            }
            else {
                screen.BackgroundColor = colourRed;
                screen.FontColor = colourWhite;

                LogScreenChange(screen, "rojo");
            }
        }


        /// <summary>
        /// LogLightChange private method
        /// Adds to the log var an entry about a given light and colour
        /// </summary>
        /// <param name="light"></param>
        /// <param name="color"></param>
        private void LogLightChange(IMyLightingBlock light, string color) {
            _logText += "Luz \"" + light.DisplayNameText + "\":" + Environment.NewLine + "    cambiada a " + color + Environment.NewLine;
        }


        /// <summary>
        /// LogScreenChange private method
        /// Adds to the log var an entry about a given light and colour
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="color"></param>
        private void LogScreenChange(IMyTextSurface screen, string color) {
            _logText += "Botón \"" + screen.DisplayName + "\": cambiado a " + color + Environment.NewLine;
        }


        /// <summary>
        /// Main public method
        /// Maing execution
        /// </summary>
        /// <param name="argument">Argument given</param>
        /// <param name="updateSource">"Who" ran the programmable block</param>
        public void Main(string argument, UpdateType updateSource) {
            ushort i;                                                                       // Just a counter

            IMyAirVent airVent = null;                                                      // Air vent block

            List<IMyTextSurfaceProvider> buttonPanels = new List<IMyTextSurfaceProvider>(); // Button pannels group
            List<IMyPowerProducer> engines = new List<IMyPowerProducer>();                  // Engines group
            List<IMyGasGenerator> gasGenerators = new List<IMyGasGenerator>();              // H2O2 generators group
            List<IMyLightingBlock> lights = new List<IMyLightingBlock>();                   // Lights group
            List<IMyReactor> reactors = new List<IMyReactor>();                             // Reactors group
            List<IMyGasTank> tanksH2 = new List<IMyGasTank>();                              // H2 tanks group
            List<IMyGasTank> tanksO2 = new List<IMyGasTank>();                              // O2 tanks group


            _logText = "";                                                                  // Cleaning previous logs


            if(_nameLights != "" || _nameButtonPannels != "") {
                if(_nameAirVent != "") {                                                    // Getting air vent block
                    airVent = (IMyAirVent) GridTerminalSystem.GetBlockWithName(_nameAirVent);
                }
                if(_nameButtonPannels != "") {                                              // Getting button pannels group
                    GridTerminalSystem.GetBlockGroupWithName(_nameButtonPannels).GetBlocksOfType<IMyTextSurfaceProvider>(buttonPanels);
                }
                if(_nameEngines != "") {                                                    // Getting engines group
                    GridTerminalSystem.GetBlockGroupWithName(_nameEngines).GetBlocksOfType<IMyPowerProducer>(engines);
                }
                if(_nameGasGenerators != "") {                                              // Getting H2O2 generators group
                    GridTerminalSystem.GetBlockGroupWithName(_nameGasGenerators).GetBlocksOfType<IMyGasGenerator>(gasGenerators);
                }
                if(_nameLights != "") {                                                     // Getting lights group
                    GridTerminalSystem.GetBlockGroupWithName(_nameLights).GetBlocksOfType<IMyLightingBlock>(lights);
                }
                if(_nameReactors != "") {                                                   // Getting reactors group
                    GridTerminalSystem.GetBlockGroupWithName(_nameReactors).GetBlocksOfType<IMyReactor>(reactors);
                }
                if(_nameTanksH2 != "") {                                                    // Getting H2 tanks group
                    GridTerminalSystem.GetBlockGroupWithName(_nameTanksH2).GetBlocksOfType<IMyGasTank>(tanksH2);
                }
                if(_nameTanksO2 != "") {                                                    // Getting O2 tanks group
                    GridTerminalSystem.GetBlockGroupWithName(_nameTanksO2).GetBlocksOfType<IMyGasTank>(tanksO2);
                }

                for(i = 0; i < lights.Count; i++) {                                         // Iterating lighgts so as to change them
                    switch(lights[i].CustomData) {                                          // Each light CustomData determines the block is tied with
                        case "AirVent":
                            if(airVent != null) {
                                SetLight(lights[i], airVent);
                            }
                            break;
                        case "Engines":
                            if(engines != null && engines[0] != null) {
                                SetLight(lights[i], engines[0]);
                            }
                            break;
                        case "Generators":
                            if(gasGenerators != null && gasGenerators[0] != null) {
                                SetLight(lights[i], gasGenerators[0]);
                            }
                            break;
                        case "Reactors":
                            if(reactors != null && reactors[0] != null) {
                                SetLight(lights[i], reactors[0]);
                            }
                            break;
                        case "TanksH2":
                            if(tanksH2 != null && tanksH2[0] != null) {
                                SetLight(lights[i], tanksH2[0]);
                            }
                            break;
                        case "TanksO2":
                            if(tanksO2 != null && tanksO2[0] != null) {
                                SetLight(lights[i], tanksO2[0]);
                            }
                            break;
                        default:
                            LogLightChange(lights[i], "indeterminado");
                            break;
                    }
                }

                if(buttonPanels.Count == 2) {
                    if(tanksO2 != null && tanksO2[0] != null) {
                        SetScreen(buttonPanels[1].GetSurface(0), tanksO2[0]);
                    }
                    if(tanksH2 != null && tanksH2[0] != null) {
                        SetScreen(buttonPanels[1].GetSurface(1), tanksH2[0]);
                    }
                    if(gasGenerators != null && gasGenerators[0] != null) {
                        SetScreen(buttonPanels[1].GetSurface(2), gasGenerators[0]);
                    }
                    if(airVent != null) {
                        SetScreen(buttonPanels[1].GetSurface(3), airVent);
                    }
                    if(reactors != null && reactors[0] != null) {
                        SetScreen(buttonPanels[0].GetSurface(0), reactors[0]);
                    }
                    if(engines != null && engines[0] != null) {
                        SetScreen(buttonPanels[0].GetSurface(1), engines[0]);
                    }
                }
                else {
                    _logText += "Grupo de paneles de botones inexistente o mal configurado" + Environment.NewLine;
                }

                if(_log) {                                                                  // If log system is active, main text will be shown on screen 0 (big one) and Script name, version and time will be shown on screen 1 (keyboard one)
                    _screens[0].WriteText(_logText);
                    _screens[1].WriteText(_scriptName + " v" + _scriptVersion + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss"));

                    _logText = _scriptName + " " + _scriptVersion + " @ " + DateTime.Now.ToString("HH:mm:ss") + ":" + Environment.NewLine + Environment.NewLine + _logText;

                    Echo(_logText);
                }
            }
            else {
                if(_log) {                                                                  // If log system is active, main text will be shown on screen 0 (big one) and Script name, version and time will be shown on screen 1 (keyboard one)
                    _logText += "INFO: Ligts and panels variables empty" + Environment.NewLine + "or missconfigured. Cannot do anything.";

                    _screens[0].WriteText(_logText);
                    _screens[1].WriteText(_scriptName + " v" + _scriptVersion + Environment.NewLine + DateTime.Now.ToString("HH:mm:ss"));

                    _logText = _scriptName + " " + _scriptVersion + " @ " + DateTime.Now.ToString("HH:mm:ss") + ":" + Environment.NewLine + Environment.NewLine + _logText;

                    Echo(_logText);
                }
            }
        }

        // Stop copying to game before this text
    }
}

