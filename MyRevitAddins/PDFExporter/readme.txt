PDFExporter
© MGTek, 2017
Post build events: 
SET rdt="$(AppData)\Bushman\Revit\2018\RevitDevTools"
SET p=$(SolutionDir)packages\
SET a=Revit2018DevTools
for /D %%x in ("%p%%a%*") do if not defined f set "f=%%x"
SET pa=%f%\lib\net46\

IF NOT "$(ConfigurationName)" == "Debug via Revit Add-In Manager" (
copy /Y "$(ProjectDir)*.addin" "$(AppData)\Autodesk\REVIT\Addins\2018"
mkdir "$(AppData)\Autodesk\REVIT\Addins\2018\$(ProjectName)\help"
xcopy /Y /E /R "$(ProjectDir)bin\$(Configuration)\*" "$(AppData)\Autodesk\REVIT\Addins\2018\$(ProjectName)"
copy /Y "$(ProjectDir)*.chm" "$(AppData)\Autodesk\REVIT\Addins\2018\$(ProjectName)\help"

mkdir %rdt%

copy /Y "%pa%*" %rdt%
) ELSE (
copy /Y "%pa%*" "$(TargetDir)"
)

Put your Revit add-in description here.