/* Course		: Java Programming
 * Author		: Nenad Samardzic
 * Date			: 04/10/2013
 * Description	: Develop an object oriented software system in Java that will keep track of pets 
 * 				  treated and boarded in an animal hospital.
 * 				  This is a class Pet implementation.
 */
package com.intro.java;

public class Pet {
	private String name, owner, color;
	public static final int MALE=1, FEMALE=2, SPAYED=3, NEUTERED=4;
	private enum sex {MALE, FEMALE, SPAYED, NEUTERED};
	protected sex petSex; //A small change of the assignment request because I wanted 
						  //to use enumerated type
	//constructor
	public Pet (String name, String ownerName, String color) {
		this.name = name;
		this.owner = ownerName;
		this.color = color;
	}
	//get name method
	String getPetName() {
		return name;
	}
	//get owner method
	String getOwnerName() {
		return owner;
	}
	//get color method
	String getColor() {
		return color;
	}
	//set sex method
	void setSex(int sexid) {
		try {
			switch(sexid) {
			case MALE:
				petSex = sex.MALE;
				break;
			case FEMALE:
				petSex = sex.FEMALE;
				break;
			case SPAYED:
				petSex = sex.SPAYED;
				break;
			case NEUTERED:
				petSex = sex.NEUTERED;
				break;
			}
		} catch(Exception e){
			throw new IllegalArgumentException("Value not allowed!");
		}
	}
	//get sex method
	String getSex() {
		return petSex.toString();
	}
	@Override
	public String toString() {
		String sHelp = this.getClass().getName().toUpperCase();
		return sHelp.substring(sHelp.lastIndexOf(".") + 1) + ":\n" + this.name + " owned by " + 
				this.owner + "\nColor: " + this.color + "\nSex: " + this.getSex() + "\n";
	}
}
