#version 330
in vec4 outColor; //Matches the output name of the VS 
out vec4 outputColor; //Out indicates that's what gets written to the screen

void main()
{
	outputColor = outColor;
}