@{
function Person(name, age) {

    if (Boolean(name))
        this.name = name;
    else
        throw new Error('A person is required to have a name');

    if (Boolean(age))
        this.age = age;

}

function Employee(name, age, idNumber, salary) {

    // Let the Person function initialize the name and age
    Person.call(this, name, age);

    // Initialize idNumber and salary
    if (Boolean(idNumber))
        this.idNumber = idNumber;
    if (Boolean(salary))
        this.salary = salary;

}

// Employee.prototype = new Person();
Employee.prototype = Person.prototype;


// Getters
Employee.prototype.getIdNumber = function () { return this.idNumber; }
Employee.prototype.getSalary = function () { return this.salary; }

// Setters
Employee.prototype.setIdNumber = function (idNumber) { this.idNumber = idNumber; }
Employee.prototype.setSalary = function (salary) { this.salary = salary; }


var rob = new Employee("Rob", "30");
rob.setSalary("100");

vLog("name", rob.name);
vLog("salary", rob.getSalary());

function vLog(name, value) {
    echo(name + "=[" + value + "]<br />");
}



return ""
}@