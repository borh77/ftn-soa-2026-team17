package com.soa.purchase.client;

import com.soa.purchase.exception.BadRequestException;
import lombok.RequiredArgsConstructor;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;
import org.springframework.web.client.HttpClientErrorException;
import org.springframework.web.client.RestClient;

@Component
@RequiredArgsConstructor
public class TourClient {
    private final RestClient.Builder restClientBuilder;

    @Value("${tour.service.url}")
    private String tourServiceUrl;

    public TourPurchaseInfo getPurchaseInfo(Long tourId) {
        try {
            return restClientBuilder.build()
                    .get()
                    .uri(tourServiceUrl + "/api/tours/" + tourId + "/purchase-info")
                    .retrieve()
                    .body(TourPurchaseInfo.class);
        } catch (HttpClientErrorException.NotFound ex) {
            throw new BadRequestException("Tura nije pronađena.");
        } catch (Exception ex) {
            throw new BadRequestException("Nije moguće proveriti podatke o turi: " + ex.getMessage());
        }
    }
}
