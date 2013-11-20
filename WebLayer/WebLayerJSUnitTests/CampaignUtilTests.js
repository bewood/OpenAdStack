/// <reference path="../weblayer/scripts/campaignutils.js" />

describe("Campaign Utils", function () {
    var player;
    var song;

    beforeEach(function () {
    });

    it("should return empty measusureInfoSet when null params are passed", function () {
        var measureSet = getEmptyMeasureInfoSet(null, null, null);

        expect(measureSet).toEqual({ MaxValuation: 0, Measures: [] });
    });

    describe("getDAAllocationIndex", function () {
        it("should return null if no associations", function () {
            var mockCampaign = {};
            var allocIndex = getDAAllocationIndex(1, 2, mockCampaign);
            expect(allocIndex).toEqual(null);
        });

        it("should return null if no History Index", function () {
            var mockCampaign = {Associations:''};
            var allocIndex = getDAAllocationIndex(1, 2, mockCampaign);
            expect(allocIndex).toEqual(null);
        });
    });

});