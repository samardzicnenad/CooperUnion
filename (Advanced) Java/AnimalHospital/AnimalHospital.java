/* Course		: Java Programming
 * Author		: Nenad Samardzic
 * Date			: 04/10/2013
 * Description  : Develop an object oriented software system in Java that will keep track of pets 
 * 				  treated and boarded in an animal hospital.
 * 				  This is a class AnimalHospital implementation.
 */
package com.intro.java;
import java.io.File;
import java.io.FileNotFoundException;
import java.util.ArrayList;
import java.util.Scanner;

public class AnimalHospital {
	public ArrayList<Pet> Animals = new ArrayList<Pet>();
	
	//helper function - transform a string into int representation
	private int getSex(String sex) {
		switch(sex.trim().toUpperCase()) {
		case "MALE":
			return 1;
		case "FEMALE":
			return 2;
		case "SPAYED":
			return 3;
		case "NEUTERED":
			return 4;
		}
		return -1;
	}
	
	//The constructor takeS a file name as an argument and reads in the file information
	public AnimalHospital(String inputFile) throws FileNotFoundException {
		String[] aTemp;
		Scanner in = new Scanner(new File(inputFile));
		String sLine;
		while (in.hasNextLine()) {
			sLine = in.nextLine();
			if (!sLine.equals("END")) {
				aTemp = sLine.split(",");
				switch(aTemp[0].toUpperCase()) {
				case "DOG":
					Dog newDog = new Dog(aTemp[1].trim(), aTemp[2].trim(), aTemp[3].trim(), aTemp[5].trim());
					newDog.setSex(this.getSex(aTemp[4].trim()));
					Animals.add(newDog);
					break;
				case "CAT":
					Cat newCat = new Cat(aTemp[1].trim(), aTemp[2].trim(), aTemp[3].trim(), aTemp[5].trim());
					newCat.setSex(this.getSex(aTemp[4].trim()));
					Animals.add(newCat);
					break;
				case "BIRD":
					Bird newBird = new Bird(aTemp[1].trim(), aTemp[2].trim(), aTemp[3].trim());
					newBird.setSex(this.getSex(aTemp[4].trim()));
					if (aTemp[4].trim().toUpperCase() == "YES") newBird.setClipped();
					Animals.add(newBird);
					break;
				default:
					throw new IllegalArgumentException("Animal not allowed!");
				}
			}
		}
		in.close();
	}
	//searches the list for every pet of a given name, and prints the pets' information
	void printPetInfoByName(String name){
		for (Pet myPet:Animals) {
			if (myPet.getPetName().trim().toUpperCase().equals(name.trim().toUpperCase()))
				System.out.println(myPet.toString());
		}
	}
	//searches the list for pets owned by the given person and prints the pets' information
	void printPetInfoByOwner(String name){
		for (Pet myPet:Animals) {
			if (myPet.getOwnerName().trim().toUpperCase().equals(name.trim().toUpperCase()))
				System.out.println(myPet.toString());
		}
	}
	//search the listfor every pet boarding at the given time and print the pets' information
	void printPetsBoarding(int month, int day, int year){
		for (Pet myPet:Animals) {
			//if(myPet instanceof Cat)
			if (myPet.getClass().toString().toUpperCase().endsWith("CAT"))
				System.out.println(((Cat)myPet).boarding(month, day, year));
			if (myPet.getClass().toString().toUpperCase().endsWith("DOG"))
				System.out.println(((Dog)myPet).boarding(month, day, year));
		}
	}
}
