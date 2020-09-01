# Rule Engine

## goals

build a framework to evaluate context (strong-typed streams) using rule expression (compiled lambda expression)

## rules

rule is defined using lambda expression against context type, it contains the following fileds
- filter: predicate expression (`Func<T, bool>`) for applicability of rule
- assert: predicate expression indicating if rule is passed
- weight: when multiple rules are applied, this value is used to generated weighted score for summary
- evaluation context: global values (reference, never serialized)
- 

complex logic can be extended using extension method, suppose we are evaluating context `Device`, we can create an extension method:
``` c#
public static class DeviceHiararchyCheck
{
    public static bool DeviceHasCircularPath(this Device device)
    {
        var allParentDevices = device.EvaluationContext.GetAllParents(device.DeviceName, out var devicesWithCircularPath);
        if (devicesWithCircularPath.Any())
        {
            device.ValidationEvidence = new ValidationEvidence
            {
                ...
            };
            return true;
        }
        return false;
    }
}
```

This method can be used in rule expression:
``` json
{
    "Name": "CircularPathCheck",
    "Filter": {
        "Left": "ChildrenDevices",
        "Operator": "IsEmpty",
        "Right": "true"
    },
    "Assert": {
        "Left": "Self.DeviceHasCircularPath()",
        "Operator": "Equals",
        "Right": "false"
    }
}
```

## Pipeline

Validation job is the unit of scheduling, it target a group that contains target context (i.e. 1000s of devices) and group of rules (selected rule targeting the same context type).

When background service dequeue a job, it uses pipeline to run validation in parallel. A pipeline contains the following 4 tasks:
1. producer: generate streams of payload (combination of context instance and rule, i.e. if there are 5000 devices and 15 rules, total of 75k payload is generated)
2. transform: compiled rule expression is used to run agains context instance, result is returned
3. batch: evaluation results for applicabe payload are collected into batch of predefined size
4. persistence: evaluation results are saved to database

## UI

### rule editor
allow user to define rule expression based on reflection

### schedule
allow user to define recursive schedule

### dashboard
allow user to define dashboard and drilldown validation results
