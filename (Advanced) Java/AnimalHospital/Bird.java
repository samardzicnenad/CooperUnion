/* Course		: Java Programming
 * Author		: Nenad Samardzic
 * Date			: 04/10/2013
 * Description  : Develop an object oriented software system in Java that will keep track of pets 
 * 				  treated and boarded in an animal hospital.
 * 				  This is a class Bird implementation.
 */
package com.intro.java;

public class Bird extends Pet {
	private boolean feathersClipped;
	//constructor
	public Bird(String name, String ownerName, String color) {
		super(name, ownerName, color);
		this.feathersClipped = false;
	}
	//get clipped method
	boolean clipped() {
		return feathersClipped;
	}
	//set clipped method
	void setClipped() {
		this.feathersClipped = true;
	}
	@Override
	public String toString() {
		return super.toString() + "Feather clipped: " + (this.clipped()?"Yes":"No") + "\n";
	}
}
