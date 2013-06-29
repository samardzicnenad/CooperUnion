/* Course		: Java Programming
 * Author		: Nenad Samardzic
 * Date			: 04/10/2013
 * Description  : Develop an object oriented software system in Java that will keep track of pets 
 * 				  treated and boarded in an animal hospital.
 * 				  This is an interface Boardable implementation.
 */
package com.intro.java;

public interface Boardable {
	public void setBoardStart(int month, int day, int year);
	public void setBoardEnd(int month, int day, int year);
	public boolean boarding(int month, int day, int year);
}