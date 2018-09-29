# MyRevitAddins
A collection af addins for Autodesk Revit which I use to help me automate tasks which the vanilla UI lacks the tools and/or workflows for.

## MEPUtils
### InsulationHandler
Utility to automatically insulate piping components as specified in an excel-sheet. Use it a lot, saves me a lot of time when modelling piping.
### FlangeCreator
Utility to put flanges on flanged in-line PipeAccessories. It is annoying to have to manually to have to put counter flanges on in-line pipe accessories, so it is now done with this utility.
### PipeCreator
Creates a pipe from a random not-connected connector on a pipe accessory or fitting. Is very convenient in some situations. If selected element a Pipe, then the routine asks for a cardinal direction and dras a pipe with elbow in that direction. Very handy when you have to draw vertical pipe segments -> is a little quicker than setting the pipe offset to a lower (higher) value and drawing.
### TotalLineLength
Calculates the total length of all selected detail and model lines.
### ConnectConnectors
Connects or disconnects adjacent connectors on one, two, multiple or all piping components, choice of operation chosen by number of selected components and/or connection status. Use this a lot, it saves me a LOT of time and is really convenient to be able to do stuff multiple times daily that I never would have had a chance to do otherwise. This functionality is definitely something that is really missing in the vanilla GUI.
### PipeInsulationVisibility
Provides a button which toggles the visibility of piping insulation in the current view with a single click. I put it in Quick Access Toolbar and use it all the time.
### MoveToDistance
Moves an element, on the same pipe as another element, to a set distance to the second element. Is very handy when placing PipeAccessories (Valves etc.) and a set distance is needed to another element (tee, valve etc.). Before I would go to a plan view and draw detail lines with the wanted length, which was very cumbersome. This utility saves time and allows one to model with set distances between elements much quicker.

## PDFExporter
Exports sheets of different sizes in a selected sheet set to PDF. The files are given a custom name. Requires BlueBeam to function and a specific setup of sheet sizes.

## PED
A small utility to help me produce components sheets (by populating shared parameters with relevant information) with PED (Pressure Equipment Directive) information for piping components.

## PlaceSupport
Places symbolic pipe support elements to facilitate export of support conditions to Caesar II. Is used in conjunction with my other addin Revit PCF Exporter.

## RibbonPanel
The entry point for all addins, creates buttons to call the functions.

## Shared
Shared library of generic useful methods.
