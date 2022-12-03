<template>
  <div class="text-center">
    <div class="text-h5 q-mt-md">Your Orders</div>
    <div class="text-positive text-h6 q-mt-lg q-mb-lg">
      {{ state.status }}
    </div>
  </div>
  <q-card class="q-ma-sm">
    <q-list highlight>
      <q-item>
        <q-item-section>
          <q-item-label>#</q-item-label>
        </q-item-section>
        <q-item-section>Date</q-item-section>
      </q-item>
      <q-item clickable v-for="order in state.orders" :key="order.id">
        <q-item-section>
          {{ order.id }}
        </q-item-section>
        <q-item-section>
          {{ formatDate(order.dateCreated) }}
        </q-item-section>
      </q-item>
    </q-list>
  </q-card>
</template>
<script>
import { reactive, onMounted } from "vue";
import { fetcher } from "../utils/apiutil";
import { formatDate } from "../utils/formatutils";

export default {
  setup() {
    let state = reactive({
      status: "",
      orders: [],
    });

    onMounted(() => {
      loadOrders();
    });

    const loadOrders = async () => {
      try {
        let customer = JSON.parse(sessionStorage.getItem("customer"));
        const payload = await fetcher(`Order/${customer.email}`);
        console.log(payload);
        if (payload.error === undefined) {
          state.orders = payload;
          state.status = `Loaded ${state.orders.length} orders`;
        } else {
          state.status = payload.error;
        }
      } catch (err) {
        console.log(err);
        state.status = `Error has occured: ${err.message}`;
      }
    };

    return {
      state,
      formatDate,
    };
  },
};
</script>
