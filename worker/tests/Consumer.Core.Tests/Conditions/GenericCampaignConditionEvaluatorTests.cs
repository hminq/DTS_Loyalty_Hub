using Campaign.Contracts.Constants;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Definitions;
using Consumer.Core.Services;
using FluentAssertions;
using Messaging.Contracts.Events;

namespace Consumer.Core.Tests.Conditions;

public sealed class GenericCampaignConditionEvaluatorTests
{
    private readonly GenericCampaignConditionEvaluator _evaluator = new();

    private static PublishedEventDefinition CreateDefinition(params EventPayloadFieldSchema[] fields)
    {
        return new PublishedEventDefinition(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TEST_EVENT",
            "test.routing.key",
            1,
            "PUBLISHED",
            DateTime.UtcNow,
            new EventPayloadSchema(
                fields,
                [
                    new EventTargetSchema(
                        "user",
                        EventTargetKinds.Customer,
                        "userId")
                ]));
    }

    private static EventPayloadFieldSchema StringField(
        string code,
        bool conditionable = true,
        bool required = true,
        string? format = null)
    {
        return new EventPayloadFieldSchema(
            code,
            EventPayloadDataTypes.String,
            format,
            required,
            conditionable);
    }

    private static EventPayloadFieldSchema NumberField(
        string code,
        bool conditionable = true,
        bool required = true)
    {
        return new EventPayloadFieldSchema(
            code,
            EventPayloadDataTypes.Number,
            null,
            required,
            conditionable);
    }

    private static EventPayloadFieldSchema BooleanField(
        string code,
        bool conditionable = true,
        bool required = true)
    {
        return new EventPayloadFieldSchema(
            code,
            EventPayloadDataTypes.Boolean,
            null,
            required,
            conditionable);
    }

    [Fact]
    public void EmptyAllArray_ReturnsMatched()
    {
        var definition = CreateDefinition(StringField("userId", format: EventPayloadFormats.Uuid));
        var payloadValues = new Dictionary<string, ValidatedPayloadValue>
        {
            ["userId"] = new("userId", EventPayloadDataTypes.String, EventPayloadFormats.Uuid, Guid.NewGuid().ToString("D"))
        };

        var result = _evaluator.Evaluate("{\"all\":[]}", definition, payloadValues);

        result.IsValid.Should().BeTrue();
        result.IsMatch.Should().BeTrue();
    }

    [Theory]
    [InlineData("{\"all\":[{\"field\":\"name\",\"operator\":\"EQUALS\",\"value\":\"antigravity\"}]}", "antigravity", true)]
    [InlineData("{\"all\":[{\"field\":\"name\",\"operator\":\"EQUALS\",\"value\":\"antigravity\"}]}", "Antigravity", false)]
    [InlineData("{\"all\":[{\"field\":\"name\",\"operator\":\"NOT_EQUALS\",\"value\":\"antigravity\"}]}", "Antigravity", true)]
    [InlineData("{\"all\":[{\"field\":\"name\",\"operator\":\"NOT_EQUALS\",\"value\":\"antigravity\"}]}", "antigravity", false)]
    [InlineData("{\"all\":[{\"field\":\"name\",\"operator\":\"CONTAINS\",\"value\":\"gravity\"}]}", "antigravity", true)]
    [InlineData("{\"all\":[{\"field\":\"name\",\"operator\":\"CONTAINS\",\"value\":\"Gravity\"}]}", "antigravity", false)]
    public void StringOperators_EvaluationMatrix(string conditionJson, string payloadName, bool expectedMatch)
    {
        var definition = CreateDefinition(
            StringField("userId", format: EventPayloadFormats.Uuid),
            StringField("name"));
        var payloadValues = new Dictionary<string, ValidatedPayloadValue>
        {
            ["userId"] = new("userId", EventPayloadDataTypes.String, EventPayloadFormats.Uuid, Guid.NewGuid().ToString("D")),
            ["name"] = new("name", EventPayloadDataTypes.String, null, payloadName)
        };

        var result = _evaluator.Evaluate(conditionJson, definition, payloadValues);

        result.IsValid.Should().BeTrue();
        result.IsMatch.Should().Be(expectedMatch);
    }

    [Fact]
    public void UuidString_CanonicalComparison()
    {
        var uuid = Guid.NewGuid();
        var lowerUuid = uuid.ToString("D");
        var upperUuid = uuid.ToString("D").ToUpperInvariant();

        var definition = CreateDefinition(
            StringField("userId", format: EventPayloadFormats.Uuid),
            StringField("refId", format: EventPayloadFormats.Uuid));
        var payloadValues = new Dictionary<string, ValidatedPayloadValue>
        {
            ["userId"] = new("userId", EventPayloadDataTypes.String, EventPayloadFormats.Uuid, lowerUuid),
            ["refId"] = new("refId", EventPayloadDataTypes.String, EventPayloadFormats.Uuid, lowerUuid)
        };

        var conditionJson = $"{{\"all\":[{{\"field\":\"refId\",\"operator\":\"EQUALS\",\"value\":\"{upperUuid}\"}}]}}";

        var result = _evaluator.Evaluate(conditionJson, definition, payloadValues);

        result.IsValid.Should().BeTrue();
        result.IsMatch.Should().BeTrue();
    }

    [Theory]
    [InlineData("EQUALS", "100.50", "100.50", true)]
    [InlineData("EQUALS", "100.50", "100.51", false)]
    [InlineData("NOT_EQUALS", "-50.25", "10.00", true)]
    [InlineData("NOT_EQUALS", "-50.25", "-50.25", false)]
    [InlineData("GT", "100", "50", true)]
    [InlineData("GT", "50", "50", false)]
    [InlineData("GTE", "50", "50", true)]
    [InlineData("GTE", "49.99", "50", false)]
    [InlineData("LT", "-10", "0", true)]
    [InlineData("LT", "0", "0", false)]
    [InlineData("LTE", "0", "0", true)]
    [InlineData("LTE", "1", "0", false)]
    [InlineData("EQUALS", "79228162514264337593543950335", "79228162514264337593543950335", true)]
    [InlineData("EQUALS", "-79228162514264337593543950335", "-79228162514264337593543950335", true)]
    public void NumberOperators_DecimalEvaluationMatrix(string op, string factStr, string condStr, bool expectedMatch)
    {
        var definition = CreateDefinition(
            StringField("userId", format: EventPayloadFormats.Uuid),
            NumberField("amount"));
        var payloadValues = new Dictionary<string, ValidatedPayloadValue>
        {
            ["userId"] = new("userId", EventPayloadDataTypes.String, EventPayloadFormats.Uuid, Guid.NewGuid().ToString("D")),
            ["amount"] = new("amount", EventPayloadDataTypes.Number, null, decimal.Parse(factStr, System.Globalization.CultureInfo.InvariantCulture))
        };

        var conditionJson = $"{{\"all\":[{{\"field\":\"amount\",\"operator\":\"{op}\",\"value\":{condStr}}}]}}";

        var result = _evaluator.Evaluate(conditionJson, definition, payloadValues);

        result.IsValid.Should().BeTrue();
        result.IsMatch.Should().Be(expectedMatch);
    }

    [Theory]
    [InlineData("EQUALS", true, true, true)]
    [InlineData("EQUALS", true, false, false)]
    [InlineData("NOT_EQUALS", true, false, true)]
    [InlineData("NOT_EQUALS", false, false, false)]
    public void BooleanOperators_EvaluationMatrix(string op, bool factBool, bool condBool, bool expectedMatch)
    {
        var definition = CreateDefinition(
            StringField("userId", format: EventPayloadFormats.Uuid),
            BooleanField("isVip"));
        var payloadValues = new Dictionary<string, ValidatedPayloadValue>
        {
            ["userId"] = new("userId", EventPayloadDataTypes.String, EventPayloadFormats.Uuid, Guid.NewGuid().ToString("D")),
            ["isVip"] = new("isVip", EventPayloadDataTypes.Boolean, null, factBool)
        };

        var conditionJson = $"{{\"all\":[{{\"field\":\"isVip\",\"operator\":\"{op}\",\"value\":{(condBool ? "true" : "false")}}}]}}";

        var result = _evaluator.Evaluate(conditionJson, definition, payloadValues);

        result.IsValid.Should().BeTrue();
        result.IsMatch.Should().Be(expectedMatch);
    }

    [Fact]
    public void AbsentOptionalField_EvaluatesToFalse_IncludingNotEquals()
    {
        var definition = CreateDefinition(
            StringField("userId", format: EventPayloadFormats.Uuid),
            StringField("optCode", required: false));
        var payloadValues = new Dictionary<string, ValidatedPayloadValue>
        {
            ["userId"] = new("userId", EventPayloadDataTypes.String, EventPayloadFormats.Uuid, Guid.NewGuid().ToString("D"))
        };

        var conditionJsonNotEquals = "{\"all\":[{\"field\":\"optCode\",\"operator\":\"NOT_EQUALS\",\"value\":\"PROMO\"}]}";
        var conditionJsonEquals = "{\"all\":[{\"field\":\"optCode\",\"operator\":\"EQUALS\",\"value\":\"PROMO\"}]}";

        var resultNotEquals = _evaluator.Evaluate(conditionJsonNotEquals, definition, payloadValues);
        var resultEquals = _evaluator.Evaluate(conditionJsonEquals, definition, payloadValues);

        resultNotEquals.IsValid.Should().BeTrue();
        resultNotEquals.IsMatch.Should().BeFalse();

        resultEquals.IsValid.Should().BeTrue();
        resultEquals.IsMatch.Should().BeFalse();
    }

    [Fact]
    public void AbsentRequiredField_ReturnsInvalid()
    {
        var definition = CreateDefinition(
            StringField("userId", format: EventPayloadFormats.Uuid),
            StringField("requiredCode"));
        var payloadValues = new Dictionary<string, ValidatedPayloadValue>
        {
            ["userId"] = new(
                "userId",
                EventPayloadDataTypes.String,
                EventPayloadFormats.Uuid,
                Guid.NewGuid().ToString("D"))
        };

        var result = _evaluator.Evaluate(
            "{\"all\":[{\"field\":\"requiredCode\",\"operator\":\"EQUALS\",\"value\":\"PROMO\"}]}",
            definition,
            payloadValues);

        result.IsValid.Should().BeFalse();
        result.IsMatch.Should().BeFalse();
        result.Condition.Should().BeNull();
    }

    [Fact]
    public void MultiplePredicates_RequiresAllToMatch()
    {
        var definition = CreateDefinition(
            StringField("userId", format: EventPayloadFormats.Uuid),
            StringField("name"),
            NumberField("score"));
        var payloadValues = new Dictionary<string, ValidatedPayloadValue>
        {
            ["userId"] = new("userId", EventPayloadDataTypes.String, EventPayloadFormats.Uuid, Guid.NewGuid().ToString("D")),
            ["name"] = new("name", EventPayloadDataTypes.String, null, "Alice"),
            ["score"] = new("score", EventPayloadDataTypes.Number, null, 100m)
        };

        var matchingJson = "{\"all\":[{\"field\":\"name\",\"operator\":\"EQUALS\",\"value\":\"Alice\"},{\"field\":\"score\",\"operator\":\"GTE\",\"value\":50}]}";
        var nonMatchingJson = "{\"all\":[{\"field\":\"name\",\"operator\":\"EQUALS\",\"value\":\"Alice\"},{\"field\":\"score\",\"operator\":\"GT\",\"value\":150}]}";

        _evaluator.Evaluate(matchingJson, definition, payloadValues).IsMatch.Should().BeTrue();
        _evaluator.Evaluate(nonMatchingJson, definition, payloadValues).IsMatch.Should().BeFalse();
    }

    [Theory]
    [InlineData("{\"all\":[{\"field\":\"name\",\"operator\":\"EQUALS\",\"value\":123}]}")] // string field with number value
    [InlineData("{\"all\":[{\"field\":\"score\",\"operator\":\"EQUALS\",\"value\":\"100\"}]}")] // number field with string value (coercion rejection)
    [InlineData("{\"all\":[{\"field\":\"isVip\",\"operator\":\"EQUALS\",\"value\":\"true\"}]}")] // boolean field with string value (coercion rejection)
    [InlineData("{\"all\":[{\"field\":\"name\",\"operator\":\"GT\",\"value\":\"Alice\"}]}")] // wrong operator for string
    [InlineData("{\"all\":[{\"field\":\"score\",\"operator\":\"CONTAINS\",\"value\":10}]}")] // wrong operator for number
    [InlineData("{\"all\":[{\"field\":\"isVip\",\"operator\":\"GT\",\"value\":true}]}")] // wrong operator for boolean
    [InlineData("{\"all\":[{\"field\":\"unknownField\",\"operator\":\"EQUALS\",\"value\":\"test\"}]}")] // unknown field
    [InlineData("{\"all\":[{\"field\":\"Name\",\"operator\":\"EQUALS\",\"value\":\"Alice\"}]}")] // case-mismatched field
    [InlineData("{\"all\":[{\"field\":\"nonCond\",\"operator\":\"EQUALS\",\"value\":\"test\"}]}")] // non-conditionable field
    [InlineData("{\"all\":[{\"field\":\"name\",\"operator\":\"IN\",\"value\":[\"a\",\"b\"]}]}")] // IN operator
    [InlineData("{\"all\":[{\"field\":\"name\",\"operator\":\"equals\",\"value\":\"Alice\"}]}")] // non-exact operator
    [InlineData("{\"all\":[{\"field\":\"name\",\"operator\":\"EQUALS\",\"value\":[\"Alice\"]}]}")] // array value
    [InlineData("{\"all\":[{\"all\":[]}]}")] // nested group
    [InlineData("{\"all\":[{\"field\":\"refId\",\"operator\":\"EQUALS\",\"value\":\"not-a-uuid\"}]}")] // invalid UUID
    [InlineData("{\"all\":[{\"field\":\"refId\",\"operator\":\"EQUALS\",\"value\":\"00000000-0000-0000-0000-000000000000\"}]}")] // empty UUID
    [InlineData("{\"all\":[{\"field\":\"score\",\"operator\":\"EQUALS\",\"value\":1e100}]}")] // outside decimal range
    [InlineData("{\"or\":[]}")] // 'or' root
    [InlineData("{\"all\":[],\"extra\":1}")] // unknown root property
    [InlineData("{\"all\":[],\"all\":[]}") ] // duplicate root property
    [InlineData("{\"all\":[{\"field\":\"name\",\"operator\":\"EQUALS\",\"value\":\"a\",\"extra\":1}]}")] // unknown predicate property
    [InlineData("{\"all\":[{\"field\":\"name\",\"field\":\"name\",\"operator\":\"EQUALS\",\"value\":\"a\"}]}")] // duplicate predicate property
    [InlineData("{\"all\":[{\"field\":\"name\",\"operator\":\"EQUALS\",\"value\":\"a\"},{\"field\":\"name\",\"operator\":\"EQUALS\",\"value\":\"a\"}]}")] // duplicate predicate
    [InlineData("not json")] // malformed json
    public void InvalidConditions_ReturnInvalid(string conditionJson)
    {
        var definition = CreateDefinition(
            StringField("userId", format: EventPayloadFormats.Uuid),
            StringField("name"),
            StringField("refId", format: EventPayloadFormats.Uuid),
            StringField("nonCond", conditionable: false),
            NumberField("score"),
            BooleanField("isVip"));
        var payloadValues = new Dictionary<string, ValidatedPayloadValue>
        {
            ["userId"] = new("userId", EventPayloadDataTypes.String, EventPayloadFormats.Uuid, Guid.NewGuid().ToString("D")),
            ["name"] = new("name", EventPayloadDataTypes.String, null, "Alice"),
            ["refId"] = new("refId", EventPayloadDataTypes.String, EventPayloadFormats.Uuid, Guid.NewGuid().ToString("D")),
            ["nonCond"] = new("nonCond", EventPayloadDataTypes.String, null, "secret"),
            ["score"] = new("score", EventPayloadDataTypes.Number, null, 100m),
            ["isVip"] = new("isVip", EventPayloadDataTypes.Boolean, null, true)
        };

        var result = _evaluator.Evaluate(conditionJson, definition, payloadValues);

        result.IsValid.Should().BeFalse();
    }
}
