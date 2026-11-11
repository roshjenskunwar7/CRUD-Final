import http from 'k6/http';
import { check } from 'k6';
import { Counter } from 'k6/metrics';

const statusCodes = new Counter('http_status_codes');

export const options = {
    vus: 500,
    duration: '60s',
};

export default function () {
    const response = http.get(`${__ENV.BASE_URL}/api/students`);

    statusCodes.add(1, {
        status: String(response.status),
    });

    if (response.status === 0 && __VU <= 3 && __ITER < 2) {
        console.error(`Request error: ${response.error}`);
    }

    check(response, {
        'status is 200': (r) => r.status === 200,
    });
}