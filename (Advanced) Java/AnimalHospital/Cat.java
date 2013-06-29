/* Course		: Java Programming
 * Author		: Nenad Samardzic
 * Date			: 04/10/2013
 * Description  : Develop an object oriented software system in Java that will keep track of pets 
 * 				  treated and boarded in an animal hospital.
 * 				  This is a class Cat implementation.
 */
package com.intro.java;

public class Cat extends Pet implements Boardable {
	private String hairLength;
	private Date boardingStart, boardingEnd;
	
	//I implemented my own Date sub class since I don't like java.util.Calendar
	private class Date {
		private int Day, Month, Year;
		//constructor
		public Date(int Day, int Month, int Year) {
			this.Day = Day;
			this.Month = Month;
			this.Year = Year;
		}
		//comparing method less than
		public boolean lessThan(Date date) {
			if (this.Year < date.Year) return true;
			else if (this.Year == date.Year) {
				if (this.Month < date.Month) return true;
				else if (this.Month == date.Month && this.Day <= date.Day) return true;
			}
			return false;
		}
		//comparing method greater than
		public boolean greaterThan(Date date) {
			if (this.Year > date.Year) return true;
			else if (this.Year == date.Year) {
				if (this.Month > date.Month) return true;
				else if (this.Month == date.Month && this.Day >= date.Day) return true;
			}
			return false;
		}
	}
	
	//constructor
	public Cat(String name, String ownerName, String color, String hairLength){
		super(name, ownerName, color);
		this.hairLength = hairLength;
	}
	//get hair length method
	public String getHairLength() {
		return hairLength;
	}
	@Override
	public String toString() {
		return super.toString() + "Hair: " + this.getHairLength() + "\n";
	}
	@Override
	public void setBoardStart(int month, int day, int year) {
		boardingStart = new Date(day, month, year);
	}
	@Override
	public void setBoardEnd(int month, int day, int year) {
		boardingEnd = new Date(day, month, year);
	}
	@Override
	public boolean boarding(int month, int day, int year) {
		Date boarding = new Date(day, month, year);
		return (boarding.greaterThan(boardingStart) && boarding.lessThan(boardingEnd));
	}
}
